using LabWorkflowManager.TFS.Common;
using Microsoft.TeamFoundation.TestManagement.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2012
{
    public class TFSTest : ITFSTest
    {

        public TFSTest(ITFSConnectivity connectivity)
        {
            this.Connectivity = connectivity as TFSConnectivity;
        }

        private TFSConnectivity connectivity;
        public ITFSConnectivity Connectivity { get { return this.connectivity; } private set { this.connectivity = value as TFSConnectivity; } }

        public ITestManagementService TestManagementService
        {
            get
            {
                if (this.Connectivity.IsConnected)
                {
                    return this.connectivity.Tpc.GetService<ITestManagementService>();
                }
                return null;
            }
        }

        public IEnumerable<ITestPlan> QueryTestPlans()
        {
            if (this.TestManagementService != null)
            {
                var testmgmtproj = this.TestManagementService.GetTeamProject(this.connectivity.TeamProjects.First().Name);
                ITestPlanCollection plans = testmgmtproj.TestPlans.Query("Select * From TestPlan");

                return plans;
            }
            return new List<ITestPlan>();
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestPlan> GetAssociatedTestPlans()
        {
            return this.QueryTestPlans().Select(o => new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestPlan() { Name = o.Name, Id = o.Id });
        }

        public IEnumerable<ITestSuiteEntry> QueryTestSuites(int testplanid)
        {
            return this.GetTestSuite(this.QueryTestPlans().Where(o => o.Id == testplanid).First().RootSuite.Entries);
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSuite> GetAssociatedTestSuites(int testplanId)
        {
            return this.QueryTestSuites(testplanId).Select(o => new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSuite() { Title = o.Title, Id = o.Id, ParentId = o.ParentTestSuite.Id });
        }

        private IEnumerable<ITestSuiteEntry> GetTestSuite(IEnumerable<ITestSuiteEntry> suites)
        {
			foreach (var suite in suites.Where(o => o.EntryType == TestSuiteEntryType.DynamicTestSuite || o.EntryType == TestSuiteEntryType.RequirementTestSuite || o.EntryType == TestSuiteEntryType.StaticTestSuite))
            {
                yield return suite;
                IStaticTestSuite staticSuite = suite.TestSuite as IStaticTestSuite;
                if (staticSuite != null)
                 {
                     foreach(var childSuite in GetTestSuite(staticSuite.Entries.Where(o => o.EntryType == TestSuiteEntryType.DynamicTestSuite || o.EntryType == TestSuiteEntryType.RequirementTestSuite || o.EntryType == TestSuiteEntryType.StaticTestSuite)))
                     {
                         yield return childSuite;
                     }
                 }
            }
        }

        public IEnumerable<ITestConfiguration> QueryTestConfiguration()
        {
            if (this.TestManagementService != null)
            {
                
                var testmgmtproj = this.TestManagementService.GetTeamProject(this.connectivity.TeamProjects.First().Name);
                return testmgmtproj.TestConfigurations.Query("Select * from TestConfiguration");
            }
            return new List<ITestConfiguration>();
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestConfiguration> GetAssociatedTestConfigurations()
        {
            return this.QueryTestConfiguration().Select(o => new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestConfiguration() { Name = o.Name, Id = o.Id });
        }

        public IEnumerable<ITestSettings> QueryTestSettings()
        {
            if (this.TestManagementService != null)
            {

                var testmgmtproj = this.TestManagementService.GetTeamProject(this.connectivity.TeamProjects.First().Name);
                return testmgmtproj.TestSettings.Query("Select * from TestSettings");
            }
            return new List<ITestSettings>();
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSettings> GetAssociatedTestSettings()
        {
            return this.QueryTestSettings().Select(o => new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSettings() { Name = o.Name, Id = o.Id });
        }
    }
}
