using Microsoft.Azure.Management.Logic.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    public class Model
    {
        public Model()
        {
            ResourceGroups = new List<ResourceGroupModel>();
        }

        public List<ResourceGroupModel> ResourceGroups { get; set; }
    }

    public class ResourceGroupModel
    {
        public ResourceGroupModel()
        {
            LogicApps = new List<LogicAppModel>();
        }
        public List<LogicAppModel> LogicApps { get; set; }

        

        public string Name { get; set; }

        public string Location { get; set; }
    }


    public class LogicAppModel
    {
        public LogicAppModel()
        {
            FailedRuns = new List<LogicAppRunModel>();
        }
        public List<LogicAppRunModel> FailedRuns { get; set; }

        public string Id { get; set; }

        public string ResourceGroupName { get; set; }
        public string Name { get; set; }

        public Workflow LogicAppManagementObject { get; set; }


        public int NoFailedRuns { get; set; }

    }
    public class LogicAppRunModel
    {
        public LogicAppRunModel()
        {
            FailedActions = new List<FailedRunActionsModel>();
        }
        public List<FailedRunActionsModel> FailedActions { get; set; }
        public string Id { get; set; }

        public string Result { get; set; }

        public bool IsError { get; set; }

        public WorkflowRun WorkflowRun { get; set; }
    }

    public class FailedRunActionsModel
    {
        public string Name { get; set; }

        public string Error { get; set; }
    }
}
