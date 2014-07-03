using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4tecture.UI.Common.ViewModels;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class MultiEnvironmentWorkflowDefinition : NotificationObjectWithValidation
    {
        public MultiEnvironmentWorkflowDefinition()
        {
            this.Id = Guid.NewGuid();
            this.MainLabWorkflowDefinition = new LabWorkflowDefinitionDetails()
            {
                LabBuildDefinitionDetails = new LabBuildDefinitionDetails(),
                SourceBuildDetails = new SourceBuildDetails(),
                LabEnvironmentDetails = new LabEnvironmentDetails(),
                DeploymentDetails = new DeploymentDetails(),
                TestDetails = new TestDetails()
            };

            this.Environments = new ObservableCollection<MultiEnvironmentWorkflowEnvironment>();
        }

        

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.MainLabWorkflowDefinition.LabBuildDefinitionDetails.Name = value;
                this.RaisePropertyChanged(() => this.Name);
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
                this.MainLabWorkflowDefinition.LabBuildDefinitionDetails.Description = value;
                this.RaisePropertyChanged(() => this.Description);
            }
        }

        private Guid id;
        public Guid Id { get { return this.id; } set { this.id = value; this.RaisePropertyChanged(() => this.Id); } }

        private LabWorkflowDefinitionDetails mainLabWorkflowDefinition;
        public LabWorkflowDefinitionDetails MainLabWorkflowDefinition
        {
            get { return this.mainLabWorkflowDefinition; }
            set
            {
                this.mainLabWorkflowDefinition = value;
                this.RaisePropertyChanged(() => this.MainLabWorkflowDefinition);
                this.AddIsDirtyObservableChildren(this.MainLabWorkflowDefinition);
            }
        }

        private ObservableCollection<MultiEnvironmentWorkflowEnvironment> environments;

        public ObservableCollection<MultiEnvironmentWorkflowEnvironment> Environments
        {
            get
            {
                return this.environments;
            }
            set
            {
                this.environments = value;
                this.RaisePropertyChanged(() => this.Environments);
                this.AddIsDirtyObservableCollection(this.Environments);
            }
        }

        public IEnumerable<LabWorkflowDefinitionDetails> GetEnvironmentSpecificLabWorkflowDefinitionDetails()
        {
            foreach (var env in this.Environments)
            {

                if (env.TestConfigurationIds != null && env.TestConfigurationIds.Count > 0)
                {
                    foreach (var testconfiguration in env.TestConfigurationIds)
                    {
                        var clone = this.MainLabWorkflowDefinition.Clone();
                        clone.LabBuildDefinitionDetails.Name = string.Format("{0} Env-{1} Config-{2}", clone.LabBuildDefinitionDetails.Name, env.EnvironmentName, testconfiguration);
                        clone.LabBuildDefinitionDetails.Description = string.Format("MultiEnvironmentWorkflowDefinition:{0}\r\n{1}", this.Id.ToString(), clone.LabBuildDefinitionDetails.Description);
                        clone.LabEnvironmentDetails.LabEnvironmentUri = env.EnvironmentUri;
                        clone.TestDetails.TestConfigurationId = testconfiguration;

                        yield return clone;
                    }
                }
                else
                {
                    throw new Exception("No testconfiguration configured!");
                }
            }
        }
    }

    public class MultiEnvironmentWorkflowEnvironment : NotificationObjectWithValidation
    {
        public MultiEnvironmentWorkflowEnvironment()
        {
            this.TestConfigurationIds = new ObservableCollection<int>();
        }

        private string environmentUri;
        public string EnvironmentUri { get { return this.environmentUri; } set { this.environmentUri = value; this.RaisePropertyChanged(() => this.EnvironmentUri); } }

        private string environmentName;
        public string EnvironmentName { get { return this.environmentName; } set { this.environmentName = value; this.RaisePropertyChanged(() => this.EnvironmentName); } }


        private ObservableCollection<int> testConfigurationIds;
        public ObservableCollection<int> TestConfigurationIds { get { return this.testConfigurationIds; } set { this.testConfigurationIds = value; this.RaisePropertyChanged(() => this.TestConfigurationIds); } }
    }
}
