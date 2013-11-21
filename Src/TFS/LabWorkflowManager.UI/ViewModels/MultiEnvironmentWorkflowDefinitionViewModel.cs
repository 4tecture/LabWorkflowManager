using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public MultiEnvironmentWorkflowDefinition Item { get; set; }

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
            get { return this.selectedEnvironments; }
            set { this.selectedEnvironments = value; this.RaisePropertyChanged(() => this.SelectedEnvironments); }
        }


        public IEnumerable<AssociatedTestPlan> AvailableTestPlans
        {
            get
            {
                return this.tfsTest.GetAssociatedTestPlans();
            }
        }

        private AssociatedTestPlan selectedTestPlan;
        public AssociatedTestPlan SelectedTestPlan
        {
            get { return this.selectedTestPlan; }
            set { this.selectedTestPlan = value; this.RaisePropertyChanged(() => this.SelectedTestPlan); this.RaisePropertyChanged(() => this.AvailableTestSuites); }
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
            get { return this.selectedTestSuites; }
            set { this.selectedTestSuites = value; this.RaisePropertyChanged(() => this.SelectedTestSuites); }
        }

        public IEnumerable<AssociatedTestSettings> AvailableTestSettings
        {
            get
            {
                return this.tfsTest.GetAssociatedTestSettings();
            }
        }

        private AssociatedTestSettings selectedTestSettings;
        public AssociatedTestSettings SelectedTestSettings
        {
            get { return this.selectedTestSettings; }
            set { this.selectedTestSettings = value; this.RaisePropertyChanged(() => this.SelectedTestSettings); }
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
            get { return this.selectedTestConfigurations; }
            set { this.selectedTestConfigurations = value; this.RaisePropertyChanged(() => this.SelectedTestConfigurations); }
        }
    }
}
