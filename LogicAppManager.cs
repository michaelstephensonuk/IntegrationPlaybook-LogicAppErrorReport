using Microsoft.Azure.Management.Logic;
using Microsoft.Azure.Management.Logic.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    public class LogicAppManager
    {
        public ServiceClientCredentials Credentials { get; set; }
        

        private LogicManagementClient _client;

        public void Connect(string subscriptionId)
        {
            _client = new LogicManagementClient(Credentials);
            _client.SubscriptionId = subscriptionId;
        }
        public List<Workflow> GetLogicAppList(string resourceGroupName)
        {
            var list = new List<Workflow>();

            var response = _client.Workflows.ListByResourceGroupAsync(resourceGroupName).Result;
            foreach (var workflow in response)
                list.Add(workflow);

            var nextPageLink = response.NextPageLink;
            while (nextPageLink != null)
            {
                nextPageLink = DownloadMoreWorkflows(list, nextPageLink);
            }

            return list;
        }

        private string DownloadMoreWorkflows(List<Workflow> list, string nextPageLink)
        {
            var response = _client.Workflows.ListByResourceGroupNext(nextPageLink);
            foreach (var workflow in response)
                list.Add(workflow);

            return response.NextPageLink;
        }


        public List<WorkflowRun> GetLogicAppRuns(string resourceGroupName, Workflow logicApp, DateTime from, DateTime to)
        {
            //startTime ge<yyyy-MM - ddTHH:mm: ss.fffZ > and startTime le<yyyy-MM - ddTHH:mm: ss.fffZ >
            var list = new List<WorkflowRun>();

            var filter = new ODataQuery<WorkflowRunFilter>();
            filter.Filter = $"status eq 'failed' and startTime ge {from.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")} and startTime le {to.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";

            var response = _client.WorkflowRuns.List(resourceGroupName, logicApp.Name, filter);
            foreach (var workflowRun in response)
                list.Add(workflowRun);

            var nextPageLink = response.NextPageLink;
            while (nextPageLink != null)
            {
                nextPageLink = DownloadMoreWorkflowRuns(list, nextPageLink);
            }

            return list;
        }

        private string DownloadMoreWorkflowRuns(List<WorkflowRun> list, string nextPageLink)
        {
            var response = _client.WorkflowRuns.ListNext(nextPageLink);
            foreach (var workflow in response)
                list.Add(workflow);

            return response.NextPageLink;
        }

        public List<WorkflowRunAction> GetLogicAppRunActionList(string resourceGroupName, Workflow logicApp, WorkflowRun run)
        {
            var list = new List<WorkflowRunAction>();

            var response = _client.WorkflowRunActions.List(resourceGroupName, logicApp.Name, run.Name);
            foreach (var workflow in response)
                list.Add(workflow);

            var nextPageLink = response.NextPageLink;
            while (nextPageLink != null)
            {
                nextPageLink = DownloadMoreWorkflowRunActions(list, nextPageLink);
            }

            return list;
        }

        private string DownloadMoreWorkflowRunActions(List<WorkflowRunAction> list, string nextPageLink)
        {
            var response = _client.WorkflowRunActions.ListNext(nextPageLink);
            foreach (var workflow in response)
                list.Add(workflow);

            return response.NextPageLink;
        }





        public string RunId { get; set; }
        public string LogicAppName { get; set; }
        public HttpResponseMessage LogicAppTriggerResponse { get; set; }
        public string LogicAppTriggerResponseStatus { get; set; }

        //public WorkflowStatus LogicAppStatus { get; set; }

        
        
        private List<Workflow> _workflows = new List<Workflow>();
        private List<WorkflowRunAction> _workFlowRunActions = new List<WorkflowRunAction>();

        public enum TriggerType
        {
            Http,
            Other
        }





        //public static LogicAppManager CreateLogicAppManager(string logicAppName)
        //{
        //    var logicAppManager = new LogicAppManager();
        //    logicAppManager.LogicAppName = logicAppName;
        //    logicAppManager.StartTracking();

        //    return logicAppManager;
        //}

        ///// <summary>
        ///// Initializes the LogicManagementClient and assigns it to a static variable. This will be used the test framework to retrieve data using Azure Logic Apps SDK
        ///// </summary>
        //public void StartTracking()
        //{
        //    _client = new LogicManagementClient(Credentials);
        //    _client.SubscriptionId = Config.SubscriptionId;

        //    RefreshWorkflowList();
        //}

        

        //public bool CheckIfLogicAppIsEnabled()
        //{
        //    bool enabled = false;

        //    foreach (var workflow in _workflows)
        //    {
        //        if (workflow.Name == LogicAppName)
        //        {
        //            if (workflow.State.ToString() == "Enabled")
        //            {
        //                enabled = true;
        //            }
        //        }
        //    }

        //    return enabled;
        //}

        //public void WaitUntilLogicAppIsComplete()
        //{
        //    WorkflowStatus? status;

        //    while (true)
        //    {
        //        var run = _client.WorkflowRuns.Get(Config.ResourceGroupName, LogicAppName, RunId);
        //        status = run.Status;
        //        LogicAppStatus = status.GetValueOrDefault();
        //        if (status == WorkflowStatus.Succeeded || status == WorkflowStatus.Failed)
        //        {
        //            Console.WriteLine("Logic App execution for run identifier: " + RunId + " is completed");
        //            break;
        //        }
        //        else
        //            Thread.Sleep(50);
        //    }
        //}

        //public void StartLogicAppWithOtherTrigger(string triggerName, HttpContent content, bool waitForLogicAppToComplete = true)
        //{
        //    LogicAppTriggerResponseStatus = ExecuteLogicAppWithOtherTriggerType(triggerName);

        //    if (waitForLogicAppToComplete)
        //    {
        //        WaitUntilLogicAppIsComplete();
        //        DownloadWorkflowRunActions();
        //    }
        //}

        //public bool WasLogicAppTriggerAccepted()
        //{
        //    if (LogicAppTriggerResponseStatus == System.Net.HttpStatusCode.Accepted.ToString())
        //        return true;
        //    else
        //        return false;
        //}

        //public bool CheckLogicAppTriggerStatus(HttpStatusCode expectedStatus)
        //{
        //    if (LogicAppTriggerResponseStatus == expectedStatus.ToString())
        //        return true;
        //    else
        //        return false;
        //}

        //public void StartLogicAppWithManualHttpTrigger(HttpContent content, bool waitForLogicAppToComplete = true)
        //{
        //    var triggerName = "manual";

        //    var url = GetCallBackUrl(triggerName);
        //    var client = new HttpClient();
        //    var uri = new Uri(url.Value);

        //    LogicAppTriggerResponse = client.PostAsync(uri, content).Result;
        //    LogicAppTriggerResponseStatus = LogicAppTriggerResponse.StatusCode.ToString();
        //    RunId = LogicAppTriggerResponse.Headers.GetValues("x-ms-workflow-run-id").First();
        //    Console.WriteLine("The Run identifier is: " + RunId);

        //    if (waitForLogicAppToComplete)
        //    {
        //        WaitUntilLogicAppIsComplete();
        //        DownloadWorkflowRunActions();
        //    }
        //}

        //private WorkflowTriggerCallbackUrl GetCallBackUrl(string triggerName)
        //{
        //    var url = _client.WorkflowTriggers.ListCallbackUrl(Config.ResourceGroupName, LogicAppName, triggerName);
        //    return url;
        //}

        //private string ExecuteLogicAppWithOtherTriggerType(string triggerName)
        //{
        //    var result = _client.WorkflowTriggers.RunAsync(Config.ResourceGroupName, LogicAppName, triggerName).Result;
        //    return result.ToString();
        //}

        //public void DownloadWorkflowRunActions()
        //{
        //    _workFlowRunActions = new List<WorkflowRunAction>();
        //    var actions = _client.WorkflowRunActions.ListAsync(Config.ResourceGroupName, LogicAppName, RunId).Result;
        //    foreach (var action in actions)
        //        _workFlowRunActions.Add(action);

        //    var nextPageLink = actions.NextPageLink;
        //    while (nextPageLink != null)
        //    {
        //        nextPageLink = DownloadMoreWorkflowRunActions(_workFlowRunActions, nextPageLink);
        //    }
        //}

        //private string DownloadMoreWorkflowRunActions(List<WorkflowRunAction> actions, string nextPageLink)
        //{
        //    var response = _client.WorkflowRunActions.ListNextAsync(nextPageLink).Result;
        //    foreach (var action in response)
        //        actions.Add(action);

        //    return response.NextPageLink;
        //}

        //public bool CheckLogicAppActionResult(string actionName, WorkflowStatus expectedStatus)
        //{
        //    var expectedStatusString = Enum.GetName(typeof(WorkflowStatus), expectedStatus);

        //    var result = _workFlowRunActions.FirstOrDefault(x => x.Name == actionName);
        //    if (result == null)
        //        return false;

        //    if (result.Status == expectedStatusString)
        //        return true;
        //    else
        //        return false;
        //}

        //public bool CheckLogicAppActionSucceeded(string actionName)
        //{
        //    var result = _workFlowRunActions.FirstOrDefault(x => x.Name == actionName);
        //    if (result == null)
        //        return false;

        //    if (result.Status == WorkflowStatus.Succeeded)
        //        return true;
        //    else
        //        return false;
        //}

        
    }

    
}
