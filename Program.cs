using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    [Command(Name = "LogicappErrorReport", Description = "Generate an excel report of errors across your logic apps")]
    [HelpOption("-?")]
    class Program
    {
        static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        private async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Welcome to tools from the Integration Playbook");
            Console.WriteLine("Check out https://www.integration-playbook.io/docs for more integration content for the Microsoft Platform");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Running the Logic App Error Report");
            Console.WriteLine($"Start Date: {StartDate}");
            Console.WriteLine($"End Date: {EndDate}");

            var errorReportArgs = new ErrorReportArgs()
            {
                OutputExcelPath = ExcelOutputPath,
                Secret = ClientSecret,
                SubscriptionId = SubscriptionId,
                TenantId = TenantId,
                WebApiApplicationId = ClientId,
                StartDate = StartDate,
                EndDate = EndDate
            };
            errorReportArgs.ResourceGroups.AddRange(ResourceGroups);


            var reportManager = new ErrorReportManager();
            reportManager.ErrorReportArgs = errorReportArgs;
            reportManager.Execute();

            Console.WriteLine("Report generation complete");
            Console.ReadLine();

            return 0;
        }

        [Required]
        [Argument(0, Name = "tenantId", Description = "The id of the Azure AD tenant")]
        private string TenantId { get; }

        [Required]
        [Argument(1, Name = "subscriptionId", Description = "The id of the Azure Subscription")]
        private string SubscriptionId { get; }

        [Required]
        [Argument(3, Name = "clientId", Description = "The client id for the Azure AD service principal")]
        private string ClientId { get; }

        [Required]
        [Argument(4, Name = "clientSecret", Description = "The secret for the Azure AD service principal")]
        private string ClientSecret { get; }

        [Required]
        [Argument(5, Name = "excelOutputPath", Description = "The output path for the excel spreadsheet")]
        private string ExcelOutputPath { get; }

        [Required]
        [Argument(6, Name = "startDate", Description = "Start date for querying data")]
        private DateTime StartDate { get; }

        [Required]
        [Argument(7, Name = "endDate", Description = "End date for querying data")]
        private DateTime EndDate { get; }


        [Required]
        [Argument(8, Name = "resourceGroups", Description = "List of resource groups to check")]
        private string[] ResourceGroups { get; }

    }


}
