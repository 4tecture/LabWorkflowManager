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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LabWorkflowManager.UI.ViewModels
{
    public class MultiEnvironmentWorkflowsViewModel : NotificationObject
    {
        private ITFSConnectivity tfsConnectivity;
        private ITFSBuild tfsBuild;
        private ITFSLabEnvironment tfsLabEnvironment;
        private ITFSTest tfsTest;
        private IWorkflowManagerStorage workflowManagerStorage;
        private IRegionManager regionManager;

        public MultiEnvironmentWorkflowsViewModel(IWorkflowManagerStorage workflowManagerStorage, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager)
        {
            this.workflowManagerStorage = workflowManagerStorage;
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            this.regionManager = regionManager;

            this.workflowManagerStorage.Load();

            this.ConnectToTfsCommand = new DelegateCommand(ConnectToTfs);
            this.AddNewDefinitionCommand = new DelegateCommand(AddNewDefinition);
            this.EditDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(EditDefinition);
            this.SaveCommand = new DelegateCommand(() => this.workflowManagerStorage.Save());
            this.DeleteDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(DeleteDefinition);
        }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.TitleManage;
            }
        }

        public ObservableCollection<MultiEnvironmentWorkflowDefinition> Definitions
        {
            get { return this.workflowManagerStorage.Definitions; }
        }

        private MultiEnvironmentWorkflowDefinition currentDefinition;
        public MultiEnvironmentWorkflowDefinition CurrentDefinition
        {
            get { return this.currentDefinition; }
            set { this.currentDefinition = value; this.RaisePropertyChanged(() => this.CurrentDefinition); }
        }

        public ICommand ConnectToTfsCommand { get; private set; }
        public ICommand AddNewDefinitionCommand { get; private set; }
        public ICommand EditDefinitionCommand { get; private set; }
        public ICommand DeleteDefinitionCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public string TeamProjectCollectionUri { get { return this.tfsConnectivity.TfsUri; } }
        public string TeamProjectName { get { return this.tfsConnectivity.TeamProjectName; } }


        private void ConnectToTfs()
        {
            this.tfsConnectivity.ConnectUI();
            this.RaisePropertyChanged(() => this.TeamProjectCollectionUri);
            this.RaisePropertyChanged(() => this.TeamProjectName);
        }

        private void AddNewDefinition()
        {
            var newDefinition = new MultiEnvironmentWorkflowDefinition() { Name = ModuleStrings.NewDefinitionname };
            this.workflowManagerStorage.Definitions.Add(newDefinition);
            this.CurrentDefinition = newDefinition;
        }

        private void EditDefinition(MultiEnvironmentWorkflowDefinition item)
        {
            this.regionManager.AddToRegion(RegionNames.MainRegion, new MultiEnvironmentWorkflowDefinitionViewModel(item, this.tfsConnectivity, this.tfsBuild, this.tfsLabEnvironment, this.tfsTest, this.regionManager));
        }

        private void DeleteDefinition(MultiEnvironmentWorkflowDefinition item)
        {
            this.workflowManagerStorage.Definitions.Remove(item);
            this.RaisePropertyChanged(() => this.Definitions);
        }
    }
}
