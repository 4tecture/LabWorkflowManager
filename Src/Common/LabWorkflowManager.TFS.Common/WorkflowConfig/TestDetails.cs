using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class TestDetails:NotificationObject
    {
        public TestDetails()
        {
            this.TestSuiteIdList = new ObservableCollection<int>();
        }

        private int testPlanId;
        private ObservableCollection<int> testSuiteIdList;
        private int testConfigurationId;
        public int TestPlanId { get { return this.testPlanId; } set { this.testPlanId = value; this.RaisePropertyChanged(() => this.TestPlanId); } }

        public ObservableCollection<int> TestSuiteIdList { get { return this.testSuiteIdList; } set { this.testSuiteIdList = value; this.RaisePropertyChanged(() => this.TestSuiteIdList); } }

        public int TestConfigurationId { get { return this.testConfigurationId; } set { this.testConfigurationId = value; this.RaisePropertyChanged(() => this.TestConfigurationId); } }

        internal TestDetails Clone()
        {
            var clone = new TestDetails();

            clone.TestPlanId = this.TestPlanId;
            clone.TestSuiteIdList = this.TestSuiteIdList;
            clone.TestConfigurationId = this.TestConfigurationId;
            clone.TestSettingsId = this.TestSettingsId;

            return clone;
        }

        public int TestSettingsId { get; set; }
    }
}
