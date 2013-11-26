using _4tecture.UI.Common.Helper;
using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            this.buildScheduleViewModel = new BuildScheduleViewModel(this.Item);

            this.Item.PropertyChanged += (sender, args) => { if (args.PropertyName.Equals("Name")) this.RaisePropertyChanged(() => this.HeaderInfo); };

            InitTestSuitesSelection();
            InitEnvironmentsSelection();
            //InitTestSettingsSelection();
            InitTestConfigurationSelection();

            this.GenerateBuildDefinitionsCommand = new DelegateCommand(GenerateBuildDefinitions);
            this.AddDeploymentScriptCommand = new DelegateCommand(AddDeploymentScript);
            this.RemoveDeploymentScriptCommand = new DelegateCommand<DeploymentScript>(RemoveDeploymentScript);
        }

        private void RemoveDeploymentScript(DeploymentScript obj)
        {
            this.Item.MainLabWorkflowDefinition.DeploymentDetails.Scripts.Remove(obj);
        }

        private void AddDeploymentScript()
        {
            this.Item.MainLabWorkflowDefinition.DeploymentDetails.Scripts.Add(new DeploymentScript());
        }

        private void InitTestSuitesSelection()
        {
            this.availableTestSuites = new SelectableCollection<AssociatedTestSuite>();
            this.availableTestSuites.SelectionChanged += TestSuitesSelectionChanged;
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged += TestSuitesCollectionChanged;
            this.TestSuitesCollectionChanged(null, null);
        }

        private void InitEnvironmentsSelection()
        {
            this.availableEnvironments = new SelectableCollection<AssociatedLabEnvironment>();
            foreach (var env in this.tfsLabEnvironment.GetAssociatedLabEnvironments())
            {
                this.availableEnvironments.Add(env);
            }
            this.availableEnvironments.SelectionChanged += EnvironmentsSelectionChanged;
            this.Item.Environments.CollectionChanged += EnvironmentsCollectionChanged;
            this.EnvironmentsCollectionChanged(null, null);
        }

        //private void InitTestSettingsSelection()
        //{
        //    this.availableTestSettings = new SelectableCollection<AssociatedTestSettings>();
        //    foreach (var ts in this.tfsTest.GetAssociatedTestSettings())
        //    {
        //        this.availableTestSettings.Add(ts);
        //    }
        //    this.availableTestSettings.SelectionChanged += TestSettingsSelectionChanged;
        //    this.Item.MainLabWorkflowDefinition.TestDetails.PropertyChanged += TestSettingsPropertyChanged;
        //    this.TestSettingsPropertyChanged(null, new PropertyChangedEventArgs("TestSettingsId"));
        //}

        private void InitTestConfigurationSelection()
        {
            this.availableTestConfigurations = new SelectableCollection<AssociatedTestConfiguration>();
            foreach (var tc in this.tfsTest.GetAssociatedTestConfigurations())
            {
                this.availableTestConfigurations.Add(tc);
            }
            this.availableTestConfigurations.SelectionChanged += TestConfigurationsSelectionChanged;
            this.Item.Environments.CollectionChanged += TestConfigurationsCollectionChanged;
            this.TestConfigurationsCollectionChanged(null, null);
        }

        private void TestConfigurationsSelectionChanged(object sender, EventArgs e)
        {
            SyncTestConfigurationsWithEnvironments(this.availableTestConfigurations.SelectedItems);
        }

        private void TestConfigurationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.availableTestConfigurations.SelectionChanged -= TestConfigurationsSelectionChanged;

            if (this.Item.Environments.Count > 0)
            {
                var selectedTestConfigurationsTmp = this.AvailableTestConfigurations.Where(o => this.Item.Environments.First().TestConfigurationIds.Contains(o.Item.Id)).Select(o => o.Item).ToList();
                this.availableTestConfigurations.SelectedItems = selectedTestConfigurationsTmp;
            }

            this.availableTestConfigurations.SelectionChanged += TestConfigurationsSelectionChanged;
        }

        //private void TestSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName.Equals("TestSettingsId"))
        //    {
        //        this.availableTestSettings.SelectionChanged -= TestSettingsSelectionChanged;
        //        this.availableTestSettings.SelectedItems = this.availableTestSettings.Where(o => o.Item.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId).Select(o => o.Item);
        //        this.availableTestSettings.SelectionChanged += TestSettingsSelectionChanged;
        //    }
        //}

        //private void TestSettingsSelectionChanged(object sender, EventArgs e)
        //{
        //    this.Item.MainLabWorkflowDefinition.TestDetails.PropertyChanged -= TestSettingsPropertyChanged;
        //    this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId = this.availableTestSettings.SelectedItems.Where(o => o.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId).Select(o => o.Id).FirstOrDefault();
        //    this.Item.MainLabWorkflowDefinition.TestDetails.PropertyChanged += TestSettingsPropertyChanged;
        //}

        private void EnvironmentsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.availableEnvironments.SelectionChanged -= EnvironmentsSelectionChanged;
            var selectedEnvironmentsTmp = this.AvailableEnvironments.Where(o => this.Item.Environments.Select(en => en.EnvironmentUri).Contains(o.Item.Uri)).Select(o => o.Item).ToList();

            this.availableEnvironments.SelectedItems = selectedEnvironmentsTmp;

            this.availableEnvironments.SelectionChanged += EnvironmentsSelectionChanged;
        }

        private void EnvironmentsSelectionChanged(object sender, EventArgs e)
        {
            this.Item.Environments.CollectionChanged -= EnvironmentsCollectionChanged;
            this.Item.Environments.Clear();
            foreach (var selectedEnv in this.availableEnvironments.SelectedItems)
            {
                this.Item.Environments.Add(new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri });
            }
            SyncTestConfigurationsWithEnvironments(this.availableTestConfigurations.SelectedItems);
            this.Item.Environments.CollectionChanged += EnvironmentsCollectionChanged;
        }

        void TestSuitesSelectionChanged(object sender, EventArgs e)
        {
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged -= TestSuitesCollectionChanged;
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Clear();
            foreach (var selectedSuite in this.availableTestSuites.SelectedItems)
            {
                this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Add(selectedSuite.Id);
            }
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged += TestSuitesCollectionChanged;
        }

        void TestSuitesCollectionChanged(object sender, EventArgs e)
        {
            this.availableTestSuites.SelectionChanged -= TestSuitesSelectionChanged;
            var selectedSuitesTmp = this.AvailableTestSuites.Where(o => this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Contains(o.Item.Id)).Select(o => o.Item).ToList();

            if (selectedSuitesTmp.Count > 0)
            {
                this.availableTestSuites.SelectedItems = selectedSuitesTmp;
            }
            this.availableTestSuites.SelectionChanged += TestSuitesSelectionChanged;
        }

        public MultiEnvironmentWorkflowDefinition Item { get; set; }

        public ICommand GenerateBuildDefinitionsCommand { get; private set; }
        public ICommand AddDeploymentScriptCommand { get; set; }
        public ICommand RemoveDeploymentScriptCommand { get; set; }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.TitleEdit + " " + Item.Name;
            }
        }

        private SelectableCollection<AssociatedLabEnvironment> availableEnvironments;
        public SelectableCollection<AssociatedLabEnvironment> AvailableEnvironments
        {
            get
            {
                return this.availableEnvironments;
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

        private AssociatedTestPlan lastSelectedTestPlan;
        private SelectableCollection<AssociatedTestSuite> availableTestSuites;
        public SelectableCollection<AssociatedTestSuite> AvailableTestSuites
        {
            get
            {
                if (lastSelectedTestPlan == null || !lastSelectedTestPlan.Equals(this.SelectedTestPlan))
                {
                    lastSelectedTestPlan = this.SelectedTestPlan;
                    this.availableTestSuites.Clear();
                    if (this.SelectedTestPlan != null)
                    {
                        foreach (var ts in this.tfsTest.GetAssociatedTestSuites(this.SelectedTestPlan.Id))
                        {
                            this.availableTestSuites.Add(ts);
                        }
                    }
                }
                return this.availableTestSuites;
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
            get
            {
                return this.AvailableTestSettings.Where(o => o.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId).FirstOrDefault();
            }
            set
            {
                this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId = value.Id;
                this.RaisePropertyChanged(() => this.SelectedTestSettings);
            }
        }


        private SelectableCollection<AssociatedTestConfiguration> availableTestConfigurations;
        public SelectableCollection<AssociatedTestConfiguration> AvailableTestConfigurations
        {
            get
            {
                return this.availableTestConfigurations;
            }
        }

        private void SyncTestConfigurationsWithEnvironments(IEnumerable<AssociatedTestConfiguration> configurations)
        {
            foreach (var env in this.Item.Environments)
            {
                env.TestConfigurationIds.Clear();
                if (configurations != null && configurations.Count() > 0)
                {
                    foreach (var tcid in configurations.Select(o => o.Id))
                    {
                        env.TestConfigurationIds.Add(tcid);
                    }
                }
            }
        }

        public IEnumerable<string> AvailableEnvironmentRoles
        {
            get
            {
                var env = this.AvailableEnvironments.SelectedItems.FirstOrDefault();
                if (env != null)
                {
                    return env.Roles;
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public IEnumerable<string> AvailableLabProcessTemplates
        {
            get
            {
                return this.tfsBuild.GetProcessTemplateFiles();
            }
        }

        public IEnumerable<string> AvailableBuildControllers
        {
            get
            {
                return this.tfsBuild.GetBuildControllers();
            }
        }

        private BuildScheduleViewModel buildScheduleViewModel;
        public BuildScheduleViewModel BuildScheduleViewModel { get { return this.buildScheduleViewModel; } }



        private void GenerateBuildDefinitions()
        {
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
