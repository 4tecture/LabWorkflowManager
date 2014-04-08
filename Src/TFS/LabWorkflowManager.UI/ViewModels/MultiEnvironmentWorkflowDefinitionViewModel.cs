using System.Windows.Data;
using _4tecture.UI.Common.Helper;
using _4tecture.UI.Common.Extensions;
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
using _4tecture.UI.Common.ViewModels;

namespace LabWorkflowManager.UI.ViewModels
{
    public class MultiEnvironmentWorkflowDefinitionViewModel : NotificationObjectWithValidation
    {
        private ITFSConnectivity tfsConnectivity;
        private ITFSBuild tfsBuild;
        private ITFSLabEnvironment tfsLabEnvironment;
        private ITFSTest tfsTest;
        private IWorkflowManagerStorage workflowManagerStorage;
        private IRegionManager regionManager;

        private object availableEnvironmentsLockObj = new object();
        private object availableTestConfigurationsLockObj = new object();
        private object availableTestSuitesLockObj = new object();
        public MultiEnvironmentWorkflowDefinitionViewModel(MultiEnvironmentWorkflowDefinition item, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager)
        {
            this.Item = item;
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            this.regionManager = regionManager;
            this.buildScheduleViewModel = new BuildScheduleViewModel(this.Item);

            this.availableEnvironments = new SelectableCollection<AssociatedLabEnvironment>();
            BindingOperations.EnableCollectionSynchronization(this.AvailableEnvironments, availableEnvironmentsLockObj);
            this.availableTestSuites = new SelectableCollection<AssociatedTestSuite>();
            BindingOperations.EnableCollectionSynchronization(this.AvailableTestSuites, availableTestSuitesLockObj);
            this.availableTestConfigurations = new SelectableCollection<AssociatedTestConfiguration>();
            BindingOperations.EnableCollectionSynchronization(this.AvailableTestConfigurations, availableTestConfigurationsLockObj);
            
            this.GenerateBuildDefinitionsCommand = new DelegateCommand(GenerateBuildDefinitions, () => !HasErrors && !this.IsGeneratingBuildDefinitions);
            this.DeleteBuildDefinitionsCommand = new DelegateCommand(DeleteExistingBuildDefinitions, () => !this.IsGeneratingBuildDefinitions);
            this.AddDeploymentScriptCommand = new DelegateCommand(AddDeploymentScript);
            this.RemoveDeploymentScriptCommand = new DelegateCommand<DeploymentScript>(RemoveDeploymentScript);

            this.Item.PropertyChanged += (sender, args) => { if (args.PropertyName.Equals("Name")) this.RaisePropertyChanged(() => this.HeaderInfo); };
            this.Item.MainLabWorkflowDefinition.SourceBuildDetails.PropertyChanged += (sender, args) => { if (args.PropertyName.Equals("QueueNewBuild")) { if (this.Item.MainLabWorkflowDefinition.SourceBuildDetails.QueueNewBuild) { this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildUri = null; this.RaisePropertyChanged(() => this.SelectedBuildtoUse); } } };

            InitializeData();

        }

        private async void InitializeData()
        {
            await Task.Run(() =>
                    {
                        IsInitializing = true;
                        InitTestSuitesSelection();
                        InitEnvironmentsSelection();
                        InitTestConfigurationSelection();
                        InitAvailableTestPlans();
                        InitAvailableLabProcessTemplates();
                        InitAvailableBuildVontrollers();
                        InitAvailableTestSettings();
                        InitAvailableSourceBuildDefinitions();
                        IsInitializing = false;
                    });
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
            this.availableTestSuites.Clear();
            this.availableTestSuites.SelectionChanged += TestSuitesSelectionChanged;
            this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.CollectionChanged += TestSuitesCollectionChanged;
            this.TestSuitesCollectionChanged(null, null);
            this.RaisePropertyChanged(() => this.AvailableTestSuites);
            this.VerifySelectedTestSuites();
        }

        private void InitEnvironmentsSelection()
        {
            this.availableEnvironments.Clear();
            foreach (var env in this.tfsLabEnvironment.GetAssociatedLabEnvironments())
            {
                this.availableEnvironments.Add(env);
            }
            this.availableEnvironments.SelectionChanged += EnvironmentsSelectionChanged;
            this.Item.Environments.CollectionChanged += EnvironmentsCollectionChanged;
            this.EnvironmentsCollectionChanged(null, null);
            this.RaisePropertyChanged(() => this.AvailableEnvironments);
            this.VerifySelectedEnvironments();
        }

        private void InitTestConfigurationSelection()
        {
            this.availableTestConfigurations.Clear();
            foreach (var tc in this.tfsTest.GetAssociatedTestConfigurations())
            {
                this.availableTestConfigurations.Add(tc);
            }
            this.availableTestConfigurations.SelectionChanged += TestConfigurationsSelectionChanged;
            this.Item.Environments.CollectionChanged += TestConfigurationsCollectionChanged;
            this.TestConfigurationsCollectionChanged(null, null);
            this.RaisePropertyChanged(() => this.AvailableTestConfigurations);
            VerifySelectedTestConfigurations();
        }

        private void TestConfigurationsSelectionChanged(object sender, EventArgs e)
        {
            SyncTestConfigurationsWithEnvironments(this.availableTestConfigurations.SelectedItems);
            VerifySelectedTestConfigurations();
        }

        private void VerifySelectedTestConfigurations()
        {
            if (this.AvailableTestConfigurations.Any(s => s.IsSelected))
            {
                this.RemoveError("AvailableTestConfigurations", ModuleStrings.ErrorNoTestConfigurationSelected);
            }
            else
            {
                this.AddError("AvailableTestConfigurations", ModuleStrings.ErrorNoTestConfigurationSelected);
            }
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
            this.RaisePropertyChanged(() => this.AvailableSnapshotsToRevert);
            this.Item.Environments.CollectionChanged += EnvironmentsCollectionChanged;

            VerifySelectedEnvironments();
        }

        private void VerifySelectedEnvironments()
        {
            if (this.AvailableEnvironments.Any(s => s.IsSelected))
            {
                this.RemoveError("AvailableEnvironments", ModuleStrings.ErrorNoEnvironmentSelected);
            }
            else
            {
                this.AddError("AvailableEnvironments", ModuleStrings.ErrorNoEnvironmentSelected);
            }
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
            VerifySelectedTestSuites();
        }

        private void VerifySelectedTestSuites()
        {
            if (this.AvailableTestSuites.Any(s => s.IsSelected))
            {
                this.RemoveError("AvailableTestSuites", ModuleStrings.ErrorNoTestSuitesSelected);
            }
            else
            {
                this.AddError("AvailableTestSuites", ModuleStrings.ErrorNoTestSuitesSelected);
            }
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
        public ICommand AddDeploymentScriptCommand { get; private set; }
        public ICommand RemoveDeploymentScriptCommand { get; private set; }

        public ICommand CloseViewCommand { get; internal set; }
        public bool IsViewClosable { get { return true; } }

        public string HeaderInfo
        {
            get
            {
                return Item.Name;
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

        private IEnumerable<AssociatedTestPlan> availableTestPlans = Enumerable.Empty<AssociatedTestPlan>();
        public IEnumerable<AssociatedTestPlan> AvailableTestPlans
        {
            get { return this.availableTestPlans; }
        }

        private void InitAvailableTestPlans()
        {
            this.availableTestPlans = this.tfsTest.GetAssociatedTestPlans();
            this.RaisePropertyChanged(() => this.AvailableTestPlans);
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

        public IEnumerable<HierarchyNode<SelectableItem<AssociatedTestSuite>>> AvailableTestSuitesHierarchy
        {
            get
            {
                return this.AvailableTestSuites.AsHierarchy(o => o.Item.Id, o => o.Item.ParentId);
            }
        }

        private IEnumerable<AssociatedTestSettings> availableTestSettings = Enumerable.Empty<AssociatedTestSettings>();
        public IEnumerable<AssociatedTestSettings> AvailableTestSettings
        {
            get
            {
                return availableTestSettings;
            }
        }

        private void InitAvailableTestSettings()
        {
            this.availableTestSettings = this.tfsTest.GetAssociatedTestSettings();
            this.RaisePropertyChanged(() => this.AvailableTestSettings);
        }

        public AssociatedTestSettings SelectedTestSettings
        {
            get
            {
                return this.AvailableTestSettings.Where(o => o.Id == this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId).FirstOrDefault();
            }
            set
            {
                if (value != null)
                {
                    this.Item.MainLabWorkflowDefinition.TestDetails.TestSettingsId = value.Id;
                    this.RemoveError("SelectedTestSettings", ModuleStrings.ErrorNoTestSettingSelected);
                }
                else
                {
                    this.AddError("SelectedTestSettings", ModuleStrings.ErrorNoTestSettingSelected);
                }
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

        private IEnumerable<string> availableLabProcessTemplates = Enumerable.Empty<string>();
        public IEnumerable<string> AvailableLabProcessTemplates
        {
            get { return availableLabProcessTemplates; }
        }

        private void InitAvailableLabProcessTemplates()
        {
            this.availableLabProcessTemplates = this.tfsBuild.GetProcessTemplateFiles();
            this.RaisePropertyChanged(() => this.AvailableLabProcessTemplates);
        }

        private IEnumerable<string> availableBuildControllers = Enumerable.Empty<string>();
        public IEnumerable<string> AvailableBuildControllers
        {
            get { return availableBuildControllers; }
        }

        private void InitAvailableBuildVontrollers()
        {
            this.availableBuildControllers = this.tfsBuild.GetBuildControllers();
            this.RaisePropertyChanged(() => this.AvailableBuildControllers);
        }

        private BuildScheduleViewModel buildScheduleViewModel;
        public BuildScheduleViewModel BuildScheduleViewModel { get { return this.buildScheduleViewModel; } }

        private List<AssociatedBuildDefinition> availableSourceBuildDefinitions = new List<AssociatedBuildDefinition>();
        private bool isGeneratingBuildDefinitions;
        private ICommand deleteBuildDefinitionsCommand;
        public IEnumerable<AssociatedBuildDefinition> AvailableSourceBuildDefinitions
        {
            get
            {

                return this.availableSourceBuildDefinitions;
            }
        }

        private void InitAvailableSourceBuildDefinitions()
        {
            this.availableSourceBuildDefinitions = this.tfsBuild.GetAssociatedDefinitions().ToList();
            this.RaisePropertyChanged(()=>this.AvailableSourceBuildDefinitions);
        }

        public AssociatedBuildDefinition SelectedSourceBuildDefinition
        {
            get
            {
                return this.AvailableSourceBuildDefinitions.Where(o => o.Uri.Equals(this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildDefinitionUri)).FirstOrDefault();
            }
            set
            {
                this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildDefinitionUri = value.Uri;
                this.RaisePropertyChanged(() => this.SelectedSourceBuildDefinition);
                this.RaisePropertyChanged(() => this.AvailableBuildsToUse);
                this.RaisePropertyChanged(() => this.SelectedBuildtoUse);
            }
        }

        public IEnumerable<AssociatedBuildDetail> AvailableBuildsToUse
        {
            get
            {
                if (this.SelectedSourceBuildDefinition != null)
                {
                    return new List<AssociatedBuildDetail>() { new AssociatedBuildDetail() { LabelName = "<LATEST>", Uri = string.Empty } }.Concat(this.SelectedSourceBuildDefinition.Builds);
                }
                return new List<AssociatedBuildDetail>();
            }
        }

        public AssociatedBuildDetail SelectedBuildtoUse
        {
            get
            {
                return this.AvailableBuildsToUse.Where(o => (o.Uri != null && o.Uri.Equals(this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildUri)) || (o.LabelName.Equals("<LATEST>") && this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildUri == null)).FirstOrDefault();
            }
            set
            {
                this.Item.MainLabWorkflowDefinition.SourceBuildDetails.BuildUri = value.Uri;
                this.RaisePropertyChanged(() => this.SelectedBuildtoUse);
            }
        }

        public IEnumerable<object> AvailableSnapshotsToRevert
        {
            get
            {
                var env = this.AvailableEnvironments.SelectedItems.FirstOrDefault();
                if (env != null)
                {
                    return env.Snapshots;
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        private async void GenerateBuildDefinitions()
        {
            this.IsGeneratingBuildDefinitions = true;

            await Task.Run(() =>
            {
                foreach (var generatedDefinitions in this.Item.GetEnvironmentSpecificLabWorkflowDefinitionDetails())
                {
                    this.tfsBuild.CreateBuildDefinitionFromDefinition(generatedDefinitions);
                }
            });

            this.IsGeneratingBuildDefinitions = false;
        }

        private async void DeleteExistingBuildDefinitions()
        {
            this.IsGeneratingBuildDefinitions = true;

            await this.tfsBuild.DeleteMultiEnvAssociatedBuildDefinitions(this.Item.Id);

            this.IsGeneratingBuildDefinitions = false;
        }

        public bool IsGeneratingBuildDefinitions
        {
            get { return this.isGeneratingBuildDefinitions; }
            set
            {
                this.isGeneratingBuildDefinitions = value;
                this.RaisePropertyChanged(() => this.IsGeneratingBuildDefinitions);
                ((DelegateCommand)this.GenerateBuildDefinitionsCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)this.DeleteBuildDefinitionsCommand).RaiseCanExecuteChanged();
            }
        }


        public ICommand DeleteBuildDefinitionsCommand
        {
            get
            {
                return this.deleteBuildDefinitionsCommand;
            }
            set
            {
                this.deleteBuildDefinitionsCommand = value;
                this.RaisePropertyChanged(() => this.DeleteBuildDefinitionsCommand);
            }
        }

        public override bool HasErrors
        {
            get { return this.HasErrorsInternal || this.Item.HasErrors; }
        }

        private bool isInitializing;

        public bool IsInitializing
        {
            get { return isInitializing; }
            set { isInitializing = value; this.RaisePropertyChanged(() => this.IsInitializing); }
        }
    }
}
