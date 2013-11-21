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
            
            this.selectedEnvironments.CollectionChanged += SelectedEnvironmentsChangedSyncToItemEnvironments;
            this.Item.Environments.CollectionChanged += ItemEnvironmentsChangedSyncToSelectedEnvironments;
            this.selectedTestSuites.CollectionChanged += SelectedTestSuitesChangedSyncToItemTestSuites;
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged += ItemTestSuitesChangedSyncToSelectedTestSuites;
            this.selectedTestConfigurations.CollectionChanged += SelectedTestConfigurationsChangedSyncToItemTestConfigurations;

            ItemEnvironmentsChangedSyncToSelectedEnvironments(null, null);
            ItemTestConfigurationsChangedSyncToSelectedTestConfigurations(null, null);
            ItemTestSuitesChangedSyncToSelectedTestSuites(null, null);
        }

       

        void SelectedEnvironmentsChangedSyncToItemEnvironments(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Item.Environments.CollectionChanged -= ItemEnvironmentsChangedSyncToSelectedEnvironments;
            this.Item.Environments.Clear();
            foreach (var selectedEnv in this.selectedEnvironments)
            {
                this.Item.Environments.Add(new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri });
            }
            SyncTestConfigurationsWithEnvironments(this.SelectedTestConfigurations);
            this.RaisePropertyChanged(() => this.SelectedEnvironments);
            this.Item.Environments.CollectionChanged += ItemEnvironmentsChangedSyncToSelectedEnvironments;
        }

        void ItemEnvironmentsChangedSyncToSelectedEnvironments(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.selectedEnvironments.CollectionChanged -= SelectedEnvironmentsChangedSyncToItemEnvironments;
            var selectedEnvironmentsTmp = this.AvailableEnvironments.Where(o => this.Item.Environments.Select(en => en.EnvironmentUri).Contains(o.Uri)).ToList();

            this.selectedEnvironments.Clear();
            if (selectedEnvironmentsTmp.Count > 0)
            {
                foreach (var env in selectedEnvironmentsTmp)
                {
                    this.selectedEnvironments.Add(env);
                }
            }
            this.selectedEnvironments.CollectionChanged += SelectedEnvironmentsChangedSyncToItemEnvironments;
        }

        void SelectedTestSuitesChangedSyncToItemTestSuites(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged -= ItemTestSuitesChangedSyncToSelectedTestSuites;
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Clear();
            foreach (var selectedSuite in this.selectedTestSuites)
            {
                this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Add(selectedSuite.Id);
            }
            
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged += ItemTestSuitesChangedSyncToSelectedTestSuites;
        }

        void ItemTestSuitesChangedSyncToSelectedTestSuites(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.selectedTestSuites.CollectionChanged -= SelectedTestSuitesChangedSyncToItemTestSuites;
            var selectedSuitesTmp = this.AvailableTestSuites.Where(o => this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Contains(o.Id)).ToList();

            this.selectedTestSuites.Clear();
            if (selectedSuitesTmp.Count > 0)
            {
                foreach (var env in selectedSuitesTmp)
                {
                    this.selectedTestSuites.Add(env);
                }
            }
            this.selectedTestSuites.CollectionChanged += SelectedTestSuitesChangedSyncToItemTestSuites;
        }
        
        void SelectedTestConfigurationsChangedSyncToItemTestConfigurations(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //this.selectedTestConfigurations.CollectionChanged -= ItemTestConfigurationsChangedSyncToSelectedTestConfigurations;
            SyncTestConfigurationsWithEnvironments(this.selectedTestConfigurations);
            //this.Item.Environments.CollectionChanged += ItemTestConfigurationsChangedSyncToSelectedTestConfigurations;
        }

        void ItemTestConfigurationsChangedSyncToSelectedTestConfigurations(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.selectedTestConfigurations.CollectionChanged -= SelectedTestConfigurationsChangedSyncToItemTestConfigurations;
            var selectedEnvironmentsTmp = this.AvailableEnvironments.Where(o => this.Item.Environments.Select(en => en.EnvironmentUri).Contains(o.Uri)).ToList();

            this.selectedTestConfigurations.Clear();
            if (this.Item.Environments.Count > 0)
            {
                var selectedTestConfigurationsTmp = this.AvailableTestConfigurations.Where(o => this.Item.Environments.First().TestConfigurationIds.Contains(o.Id));
                foreach (var env in selectedTestConfigurationsTmp)
                {
                    this.selectedTestConfigurations.Add(env);
                }
            }
            this.selectedTestConfigurations.CollectionChanged += SelectedTestConfigurationsChangedSyncToItemTestConfigurations;

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

        private ObservableCollection<AssociatedTestSuite> selectedTestSuites;
        public ObservableCollection<AssociatedTestSuite> SelectedTestSuites
        {
            get {
                return this.selectedTestSuites;
            }
            set
            {
                this.selectedTestSuites = value;
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

        private ObservableCollection<AssociatedTestConfiguration> selectedTestConfigurations;
        public ObservableCollection<AssociatedTestConfiguration> SelectedTestConfigurations
        {
            get {
                return this.selectedTestConfigurations;
                
            }
            set
            {
                this.selectedTestConfigurations = value;
                this.RaisePropertyChanged(() => this.SelectedTestConfigurations);
            }
        }

        private void SyncTestConfigurationsWithEnvironments(ObservableCollection<AssociatedTestConfiguration> configurations)
        {
            foreach (var env in this.Item.Environments)
            {
                env.TestConfigurationIds.Clear();
                if (configurations != null && configurations.Count > 0)
                {
                    foreach (var tcid in configurations.Select(o => o.Id))
                    {
                        env.TestConfigurationIds.Add(tcid);
                    }
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
