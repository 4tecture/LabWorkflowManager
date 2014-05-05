﻿using System.Windows.Data;
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
using System.Collections;

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

            this.AvailableTestSuites = new SelectableCollection<AssociatedTestSuite>();
            this.AvailableEnvironments = new SelectableCollection<AssociatedLabEnvironment>();
            
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

                        this.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName.Equals("SelectedTestPlan"))
                            {
                                this.AvailableTestSuites.RefreshSelectableItems(this.GetTestSuitesFromTestPlan(), this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList.Select(testSuiteId => new AssociatedTestSuite() { Id = testSuiteId }));
                            }

                            if (args.PropertyName.Equals("AvailableTestSuites"))
                            {
                                this.AvailableTestSuites.CollectionChanged += (s, a) => this.RaisePropertyChanged(() => this.AvailableTestSuitesHierarchy);
                            }
                        };

                        InitAvailableTestPlans();
                        
                        this.AvailableTestSuites = SelectableCollection<AssociatedTestSuite>.InitSelectableUIList(
                    this.GetTestSuitesFromTestPlan(),
                    this.Item.MainLabWorkflowDefinition.TestDetails.TestSuiteIdList,
                    selectedTestSuite => selectedTestSuite.Id,
                    testSuiteId => new AssociatedTestSuite() { Id = testSuiteId},
                    this.VerifySelectedTestSuites);
                        this.RaisePropertyChanged(() => this.AvailableTestSuitesHierarchy);


                        this.AvailableEnvironments = SelectableCollection<AssociatedLabEnvironment>.InitSelectableUIList(
                    this.tfsLabEnvironment.GetAssociatedLabEnvironments(),
                    this.Item.Environments,
                    selectedEnv => new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri },
                    workflowEnv => new AssociatedLabEnvironment() { Uri = workflowEnv.EnvironmentUri },
                    this.VerifySelectedEnvironments);

                        this.AvailableEnvironments.SelectionChanged += (sender, args) => this.RaisePropertyChanged(() => this.AvailableSnapshotsToRevert);

                    //    this.AvailableTestConfigurations = SelectableCollection<AssociatedTestConfiguration>.InitSelectableUIList(
                    //this.tfsTest.GetAssociatedTestConfigurations(),
                    //this.Item.Environments,
                    //selectedEnv => new MultiEnvironmentWorkflowEnvironment() { EnvironmentName = selectedEnv.Name, EnvironmentUri = selectedEnv.Uri },
                    //workflowEnv => new AssociatedLabEnvironment() { Uri = workflowEnv.EnvironmentUri },
                    //this.VerifySelectedEnvironments);

                        

                        InitAvailableLabProcessTemplates();
                        InitAvailableBuildControllers();
                        InitAvailableTestSettings();
                        InitAvailableSourceBuildDefinitions();

                        this.VerifyAll();
                        
                        IsInitializing = false;
                    });
        }

        private void VerifyAll()
        {
            this.VerifySelectedEnvironments();
            //this.VerifySelectedTestConfigurations();
            this.VerifySelectedTestSuites();
        }

        private void RemoveDeploymentScript(DeploymentScript obj)
        {
            this.Item.MainLabWorkflowDefinition.DeploymentDetails.Scripts.Remove(obj);
        }

        private void AddDeploymentScript()
        {
            this.Item.MainLabWorkflowDefinition.DeploymentDetails.Scripts.Add(new DeploymentScript());
        }

        

        //private void VerifySelectedTestConfigurations()
        //{
        //    if (this.AvailableTestConfigurations.Any(s => s.IsSelected))
        //    {
        //        this.RemoveError("AvailableTestConfigurations", ModuleStrings.ErrorNoTestConfigurationSelected);
        //    }
        //    else
        //    {
        //        this.AddError("AvailableTestConfigurations", ModuleStrings.ErrorNoTestConfigurationSelected);
        //    }
        //}

        

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
            set
            {
                this.availableEnvironments = value;
                this.RaisePropertyChanged(() => this.AvailableEnvironments);
                this.RaisePropertyChanged(() => this.AvailableSnapshotsToRevert);
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
            this.RaisePropertyChanged(() => this.SelectedTestPlan);
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

        private SelectableCollection<AssociatedTestSuite> availableTestSuites;
        public SelectableCollection<AssociatedTestSuite> AvailableTestSuites
        {
            get { return availableTestSuites; }
            set { this.availableTestSuites = value; this.RaisePropertyChanged(()=> this.AvailableTestSuites); }
        }

        private IEnumerable<AssociatedTestSuite> GetTestSuitesFromTestPlan()
        {
            if (this.SelectedTestPlan != null)
            {
                var res = new List<AssociatedTestSuite>();
                if (this.SelectedTestPlan != null)
                {
                    foreach (var ts in this.tfsTest.GetAssociatedTestSuites(this.SelectedTestPlan.Id))
                    {
                        res.Add(ts);
                    }
                }
                return res;
            }
            return Enumerable.Empty<AssociatedTestSuite>();
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
            this.RaisePropertyChanged(() => this.SelectedTestSettings);
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
            set { this.availableTestConfigurations = value; this.RaisePropertyChanged(()=>this.AvailableTestConfigurations); }
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

        private void InitAvailableBuildControllers()
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
            this.RaisePropertyChanged(() => this.AvailableSourceBuildDefinitions);
            this.RaisePropertyChanged(() => this.SelectedSourceBuildDefinition);
            this.RaisePropertyChanged(() => this.AvailableBuildsToUse);
            this.RaisePropertyChanged(() => this.SelectedBuildtoUse);
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
                if (this.AvailableEnvironments != null && this.AvailableEnvironments.SelectedItems.Any())
                {
                    var intersection = this.AvailableEnvironments.SelectedItems.Skip(1).Aggregate(
                        new HashSet<string>(
                            this.AvailableEnvironments.SelectedItems.First().Snapshots),
                            (h, e) =>
                            {
                                h.IntersectWith(e.Snapshots);
                                return h;
                            });
                    return intersection;
                }
                return Enumerable.Empty<object>();
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
