using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    public class ErrorReportManager
    {

        public ErrorReportArgs ErrorReportArgs { get; set; }


        public void Execute()
        {

            var authManager = new AuthManager();
            var credential = authManager.GetAzureCredentials(ErrorReportArgs.TenantId, ErrorReportArgs.WebApiApplicationId, ErrorReportArgs.Secret);

            var logicAppManager = new LogicAppManager();
            logicAppManager.Credentials = credential;
            logicAppManager.Connect(ErrorReportArgs.SubscriptionId);

            var model = new Model();
            DownloadLogieAppsToModel(ErrorReportArgs, logicAppManager, model);

            //Download Logic App Runs for each Logic App
            foreach (var resourceGroupModel in model.ResourceGroups)
            {
                var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
                Parallel.ForEach(resourceGroupModel.LogicApps, options, x => {
                    Console.WriteLine($"Downloading runs for Logic App: {x.Name}");
                    DownloadLogicAppRunToModel(ErrorReportArgs.StartDate, ErrorReportArgs.EndDate, logicAppManager, resourceGroupModel, x);

                    x.NoFailedRuns = x.FailedRuns.Count;
                });
            }

            ConvertToExcel(model, ErrorReportArgs.OutputExcelPath);
        }

        private void ConvertToExcel(Model model, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var fileInfo = new FileInfo(outputPath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                //Summary Sheet
                var logicAppSummaryWorksheet = package.Workbook.Worksheets.Add("Error Summary");

                logicAppSummaryWorksheet.Cells[1, 1].Value = "Resource Group";
                logicAppSummaryWorksheet.Cells[1, 2].Value = "Logic App Name";
                logicAppSummaryWorksheet.Cells[1, 3].Value = "No. Errors";

                
                //Put all logic apps in a temp list so we can reverse sort by failed
                var tempLogicAppModelList = (from resourceGroupModel in model.ResourceGroups
                                             from logicAppModel in resourceGroupModel.LogicApps
                                             select logicAppModel).ToList();

                //Reverse sort by no failed runs then display add to spreadsheet
                tempLogicAppModelList = tempLogicAppModelList.OrderByDescending(x => x.NoFailedRuns).ToList();

                var rowIndex = 2;
                foreach (var logicAppModel in tempLogicAppModelList)
                {
                    logicAppSummaryWorksheet.Cells[rowIndex, 1].Value = logicAppModel.ResourceGroupName;
                    logicAppSummaryWorksheet.Cells[rowIndex, 2].Value = logicAppModel.Name;
                    logicAppSummaryWorksheet.Cells[rowIndex, 3].Value = logicAppModel.NoFailedRuns;

                    rowIndex++;
                }
                
                

                //Errors List
                var logicAppErrorsWorksheet = package.Workbook.Worksheets.Add("Errors");
                logicAppErrorsWorksheet.Cells[1, 1].Value = "Resource Group";
                logicAppErrorsWorksheet.Cells[1, 2].Value = "Logic App Name";
                logicAppErrorsWorksheet.Cells[1, 3].Value = "Run ID";
                logicAppErrorsWorksheet.Cells[1, 4].Value = "Result";
                logicAppErrorsWorksheet.Cells[1, 5].Value = "Start";
                logicAppErrorsWorksheet.Cells[1, 6].Value = "End";

                rowIndex = 2;
                foreach (var resourceGroupModel in model.ResourceGroups)
                {
                    foreach (var logicAppModel in resourceGroupModel.LogicApps)
                    {
                        foreach (var runModel in logicAppModel.FailedRuns)
                        {
                            logicAppErrorsWorksheet.Cells[rowIndex, 1].Value = resourceGroupModel.Name;
                            logicAppErrorsWorksheet.Cells[rowIndex, 2].Value = logicAppModel.Name;
                            logicAppErrorsWorksheet.Cells[rowIndex, 3].Value = runModel.Id;
                            logicAppErrorsWorksheet.Cells[rowIndex, 4].Value = runModel.Result;
                            logicAppErrorsWorksheet.Cells[rowIndex, 5].Value = runModel.WorkflowRun.StartTime.Value.ToString();
                            logicAppErrorsWorksheet.Cells[rowIndex, 6].Value = runModel.WorkflowRun.EndTime.Value.ToString();

                            rowIndex++;
                        }
                    }
                }


                //Error Actions
                var errorActionsWorksheet = package.Workbook.Worksheets.Add("Error Actions");
                errorActionsWorksheet.Cells[1, 1].Value = "Resource Group";
                errorActionsWorksheet.Cells[1, 2].Value = "Logic App Name";
                errorActionsWorksheet.Cells[1, 3].Value = "Run ID";
                errorActionsWorksheet.Cells[1, 4].Value = "Action Name";
                errorActionsWorksheet.Cells[1, 5].Value = "Error";
                errorActionsWorksheet.Cells[1, 6].Value = "Start";
                errorActionsWorksheet.Cells[1, 7].Value = "End";

                rowIndex = 2;
                foreach (var resourceGroupModel in model.ResourceGroups)
                {

                    foreach (var logicAppModel in resourceGroupModel.LogicApps)
                    {
                        foreach (var runModel in logicAppModel.FailedRuns)
                        {
                            foreach (var actionModel in runModel.FailedActions)
                            {
                                errorActionsWorksheet.Cells[rowIndex, 1].Value = resourceGroupModel.Name;
                                errorActionsWorksheet.Cells[rowIndex, 2].Value = logicAppModel.Name;
                                errorActionsWorksheet.Cells[rowIndex, 3].Value = runModel.Id;
                                errorActionsWorksheet.Cells[rowIndex, 4].Value = actionModel.Name;
                                errorActionsWorksheet.Cells[rowIndex, 5].Value = actionModel.Error;
                                errorActionsWorksheet.Cells[rowIndex, 6].Value = runModel.WorkflowRun.StartTime.Value.ToString();
                                errorActionsWorksheet.Cells[rowIndex, 7].Value = runModel.WorkflowRun.EndTime.Value.ToString();

                                rowIndex++;
                            }
                        }
                    }
                }

                package.Save();
            }
        }
        private void DownloadLogicAppRunToModel(DateTime startDate, DateTime endDate, LogicAppManager logicAppManager, ResourceGroupModel resourceGroupModel, LogicAppModel logicAppModel)
        {
            var runList = logicAppManager.GetLogicAppRuns(resourceGroupModel.Name, logicAppModel.LogicAppManagementObject, startDate, endDate);
            if (runList.Count > 0)
                Console.WriteLine($"\t\tLogic app has failed runs.  Checking for actions");

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 3 };
            Parallel.ForEach(runList, options, run => {

                var runModel = new LogicAppRunModel();
                runModel.Id = run.Name;
                runModel.IsError = false;
                runModel.Result = run.Status;
                runModel.WorkflowRun = run;

                if (run.Status.ToLower() == "failed")
                {
                    Console.WriteLine($"\tLogic App Failed: {run.Name} : {run.StartTime.Value.ToString()}");

                    runModel.IsError = true;
                    logicAppModel.FailedRuns.Add(runModel);
                    logicAppModel.NoFailedRuns++;

                    var actionList = logicAppManager.GetLogicAppRunActionList(resourceGroupModel.Name, logicAppModel.LogicAppManagementObject, run);
                    Console.WriteLine($"\t\tChecking for actions {run.Name}");
                    foreach (var action in actionList)
                    {
                        if (action.Status.ToLower() == "failed")
                        {
                            var actionModel = new FailedRunActionsModel();
                            actionModel.Name = action.Name;

                            var errorMessageJson = JsonConvert.SerializeObject(action.Error);
                            try
                            {
                                var errorDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorMessageJson);

                                if (errorDictionary != null)
                                {
                                    if (errorDictionary.ContainsKey("message"))
                                        actionModel.Error = errorDictionary["message"];
                                }
                            }
                            catch (Exception)
                            {
                                //Swallow error incase error object not as expected
                            }

                            Console.WriteLine($"\t\tAction Failed: {action.Name}");

                            runModel.FailedActions.Add(actionModel);
                        }
                    }
                }
                else
                {
                    //Ignore for now
                }
            });
        }
        private void DownloadLogieAppsToModel(ErrorReportArgs errorReportArgs, LogicAppManager logicAppManager, Model model)
        {
            //Get all logic apps into model
            Console.WriteLine("Downloading logic app list");
            foreach (var resourceGroup in errorReportArgs.ResourceGroups)
            {
                var resourceGroupModel = new ResourceGroupModel();
                resourceGroupModel.Name = resourceGroup;
                model.ResourceGroups.Add(resourceGroupModel);

                var logicApps = logicAppManager.GetLogicAppList(resourceGroup);
                foreach (var logicApp in logicApps)
                {
                    var logicAppModel = new LogicAppModel();
                    logicAppModel.Id = logicApp.Id;
                    logicAppModel.LogicAppManagementObject = logicApp;
                    logicAppModel.Name = logicApp.Name;
                    logicAppModel.ResourceGroupName = resourceGroupModel.Name;

                    resourceGroupModel.LogicApps.Add(logicAppModel);
                }

                Console.WriteLine($"Completed downloading logic app list for resource group: {resourceGroup}");
            }
        }
    }
}
