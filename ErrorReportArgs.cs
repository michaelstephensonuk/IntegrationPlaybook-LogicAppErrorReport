using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    
    public class ErrorReportArgs
    {
        public ErrorReportArgs()
        {
            ResourceGroups = new List<string>();
        }
        public List<string> ResourceGroups { get; set; }

        public string SubscriptionId { get; set; }

        public string TenantId { get; set; }

        public string WebApiApplicationId { get; set; }

        public string Secret { get; set; }

        public string OutputExcelPath { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
