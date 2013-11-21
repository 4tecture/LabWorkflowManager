using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LabWorkflowManager.UI.ViewModels
{
    public class MultiEnvironmentWorkflowDefinitionViewModel : NotificationObject
    {
        private ITFSConnectivity tfsConnectivity;
        private ITFSBuild tfsBuild;
        private ITFSLabEnvironment tfsLabEnvironment;
        private ITFSTest tfsTest;
        private IWorkflowManagerStorage workflowManagerStorage;
        private IRegionManager regionManager;
        public MultiEnvironmentWorkflowDefinitionViewModel(MultiEnvironmentWorkflowDefinition item, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager)
        {
            this.Item = item;
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            this.regionManager = regionManager;

            this.Item.PropertyChanged += (sender, args) => { if (args.PropertyName.Equals("Name")) this.RaisePropertyChanged(() => this.HeaderInfo); };

            this.SelectedEnvironments = new ObservableCollection<AssociatedLabEnvironment>();
            this.SelectedTestSuites = new ObservableCollection<AssociatedTestSuite>();
            this.SelectedTestConfigurations = new ObservableCollection<AssociatedTestConfiguration>();

            this.GenerateBuildDefinitionsCommand = new DelegateCommand(GenerateBuildDefinitions);
            
            this.selectedEnvironments.CollectionChanged += selectedEnvironments_CollectionChanged;
            this.Item.Environments.CollectionChanged += Environments_CollectionChanged;
        }

       

        void selectedEnvironments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //this.Item.Environments.Clear();
            //foreach (var selectedEnv in value)
            //{
            //    this.Item.Environments.Add(new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri });
            //}
            //SyncTestConfigurationsWithEnvironments(this.SelectedTestConfigurations);
            //this.RaisePropertyChanged(() => this.SelectedEnvironments); 

            this.Item.Environments.CollectionChanged -= Environments_CollectionChanged;
            this.Item.Environments.Clear();
            foreach (var selectedEnv in this.selectedEnvironments)
            {
                this.Item.Environments.Add(new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri });
            }
            SyncTestConfigurationsWithEnvironments(this.SelectedTestConfigurations);
            this.RaisePropertyChanged(() => this.SelectedEnvironments);
            this.Item.Environments.CollectionChanged += Environments_CollectionChanged;
        }

        void Environments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var selectedEnvironmentsTmp= this.AvailableEnvironments.Where(o => this.Item.Environments.Select(e => e.EnvironmentUri).Contains(o.Uri)).ToList(); 
            //if(selectedEnvironmentsTmp.Count > 0)
            //{
            //    return new ObservableCollection<AssociatedLabEnvironment>(selectedEnvironmentsTmp);
            //}
            //else
            //{
            //    return new ObservableCollection<AssociatedLabEnvironment>();
            //}
            this.selectedEnvironments.CollectionChanged -= selectedEnvironments_CollectionChanged;
            var selectedEnvironmentsTmp = this.AvailableEnvironments.Where(o => this.Item.Environments.Select(en => en.EnvironmentUri).Contains(o.Uri)).ToList();

            this.selectedEnvironments.Clear();
            if (selectedEnvironmentsTmp.Count > 0)
            {
                foreach (var env in selectedEnvironmentsTmp)
                {
                    this.selectedEnvironments.Add(env);
                }
            }
            this.selectedEnvironments.CollectionChanged += selectedEnvironments_CollectionChanged;
        }

        



        public MultiEnvironmentWorkflowDefinition Item { get; set; }

        public ICommand GenerateBuildDefinitionsCommand { get; private set; }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.TitleEdit + " " + Item.Name;
            }
        }

        public IEnumerable<AssociatedLabEnvironment> AvailableEnvironments
        {
            get
            {
                return this.tfsLabEnvironment.GetAssociatedLabEnvironments();
            }
        }

        private ObservableCollection<AssociatedLabEnvironment> selectedEnvironments;
        public ObservableCollection<AssociatedLabEnvironment> SelectedEnvironments
        {
            get { 
                
                return this.selectedEnvironments;
            }
            set { 
                
                this.selectedEnvironments = value;
            }
        }


        public IEnumerable<AssociatedTestPlan> AvailableTestPlans
        {
            get
            {
                return this.tfsTest.GetAssociatedTestPlans();
            }
        }


        public AssociatedTestPlan SelectedTestPlan
        {
            get { return this.AvailableTestPlans.Where(o => o.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestPlanId).FirstOrDefault(); }
            set
            {
                this.Item.MainLabWorkflowDefinition.TestDetails.TestPlanId = value.Id;
                this.RaisePropertyChanged(() => this.SelectedTestPlan); this.RaisePropertyChanged(() => this.AvailableTestSuites);
            }
        }

        public IEnumerable<AssociatedTestSuite> AvailableTestSuites
        {
            get
            {
                if (this.SelectedTestPlan != null)
                {
                    return this.tfsTest.GetAssociatedTestSuites(this.SelectedTestPlan.Id);
                }
                return new List<AssociatedTestSuite>();
            }
        }


        public ObservableCollection<AssociatedTestSuite> SelectedTestSuites
        {
            get { var selectedSuitesTmp = this.AvailableTestSuites.Where(o => this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Contains(o.Id));
                return new ObservableCollection<AssociatedTestSuite>(selectedSuitesTmp);
            }
            set
            {
                if (value.Count > 0)
                {
                    this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList = new ObservableCollection<int>(value.Select(o => o.Id));
                }
                else
                {
                    this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList = new ObservableCollection<int>();
                }
                this.RaisePropertyChanged(() => this.SelectedTestSuites);
            }
        }

        public IEnumerable<AssociatedTestSettings> AvailableTestSettings
        {
            get
            {
                return this.tfsTest.GetAssociatedTestSettings();
            }
        }

        public AssociatedTestSettings SelectedTestSettings
        {
            get { 
                return this.AvailableTestSettings.Where(o => o.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId).FirstOrDefault(); }
            set
            {
                this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId = value.Id;
                this.RaisePropertyChanged(() => this.SelectedTestSettings);
            }
        }

        public IEnumerable<AssociatedTestConfiguration> AvailableTestConfigurations
        {
            get
            {
                return this.tfsTest.GetAssociatedTestConfigurations();
            }
        }

        public ObservableCollection<AssociatedTestConfiguration> SelectedTestConfigurations
        {
            get {
                if (this.Item.Environments.Count > 0)
                {
                    var selectedTestConfigurationsTmp = this.AvailableTestConfigurations.Where(o => this.Item.Environments.First().TestConfigurationIds.Contains(o.Id));
                    return new ObservableCollection<AssociatedTestConfiguration>(selectedTestConfigurationsTmp);
                }
                return new ObservableCollection<AssociatedTestConfiguration>();
            }
            set
            {
                SyncTestConfigurationsWithEnvironments(value);
            }
        }

        private void SyncTestConfigurationsWithEnvironments(ObservableCollection<AssociatedTestConfiguration> configurations)
        {
            foreach (var env in this.Item.Environments)
            {
                if (configurations != null && configurations.Count > 0)
                {
                    env.TestConfigurationIds = new ObservableCollection<int>(configurations.Select(o => o.Id));
                }
                else
                {
                    env.TestConfigurationIds = new ObservableCollection<int>();
                }
                this.RaisePropertyChanged(() => this.SelectedTestConfigurations);
            }
        }

        private void GenerateBuildDefinitions()
        {
            this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ControllerName = "VSALM";
            this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ProcessTemplateFilename = "LabDefaultTemplate.11.xaml";
            this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ContinuousIntegrationType = TFS.Common.WorkflowConfig.BuildDefinitionContinuousIntegrationType.None;

            this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildDefinitionUri = "vstfs:///Build/Definition/1";

            this.Item.MainLabWorkflowDefinition.DeploymentDetails.Scripts = new ObservableCollection<TFS.Common.WorkflowConfig.DeploymentScript>() { new TFS.Common.WorkflowConfig.DeploymentScript() { Role = "Desktop Client", Script = @"notepad.exe", WorkingDirectory = @"C:\temp" } };


            var existingBuildDefinitions = this.tfsBuild.GetMultiEnvAssociatedBuildDefinitions(this.Item.Id).ToList();
            if (existingBuildDefinitions != null && existingBuildDefinitions.Count > 0)
            {
                this.tfsBuild.DeleteBuildDefinition(existingBuildDefinitions.Select(o => new Uri(o.Uri)).ToArray());
            }

            foreach (var generatedDefinitions in this.Item.GetEnvironmentSpecificLabWorkflowDefinitionDetails())
            {
                this.tfsBuild.CreateBuildDefinitionFromDefinition(generatedDefinitions);
            }
        }
    }
}
