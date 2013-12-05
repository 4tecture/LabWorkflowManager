using _4tecture.UI.Common.Helper;
using _4tecture.UI.Common.Services;
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
        private IFileDialogService fileDialogService;

        public MultiEnvironmentWorkflowsViewModel(IWorkflowManagerStorage workflowManagerStorage, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager, IFileDialogService fileDialogService)
        {
            this.workflowManagerStorage = workflowManagerStorage;
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            this.regionManager = regionManager;
            this.fileDialogService = fileDialogService;

            if (!string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile))
            {
                this.workflowManagerStorage.Load(this.CurrentWorkflowDefinitionFile);
                this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile);
            }

            if (this.workflowManagerStorage.LastTFSConnection != null)
            {
                this.tfsConnectivity.Connect(this.workflowManagerStorage.LastTFSConnection.Uri, this.workflowManagerStorage.LastTFSConnection.Project);
                this.RaisePropertyChanged(() => this.TeamProjectCollectionUri);
                this.RaisePropertyChanged(() => this.TeamProjectName);
                this.RaisePropertyChanged(() => this.IsConnectedToTfs);
            }

            this.ConnectToTfsCommand = new DelegateCommand(ConnectToTfs);
            this.AddNewDefinitionCommand = new DelegateCommand(AddNewDefinition);
            this.EditDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(EditDefinition);
            this.NewCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.SaveFile(out filepath)) { this.workflowManagerStorage.New(filepath); this.RaisePropertyChanged(()=>this.CurrentWorkflowDefinitionFile); } });
            this.LoadCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.SaveFile(out filepath)) { this.workflowManagerStorage.Load(filepath); this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } });
            this.SaveCommand = new DelegateCommand(() => this.workflowManagerStorage.Save(this.CurrentWorkflowDefinitionFile), () => !string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile));
            this.SaveAsCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.SaveFile(out filepath)) { this.workflowManagerStorage.Save(filepath); this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } }, () => !string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile));
            this.DeleteDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(DeleteDefinition);
        }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.TitleManage;
            }
        }

        public string CurrentWorkflowDefinitionFile
        {
            get { return this.workflowManagerStorage.CurrentDefinitionFile; }
        }

        public bool IsConnectedToTfs
        {
            get
            {
                return this.tfsConnectivity.IsConnected;
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

        public ICommand NewCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SaveAsCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }

        public string TeamProjectCollectionUri { get { return this.tfsConnectivity.TfsUri; } }
        public string TeamProjectName { get { return this.tfsConnectivity.TeamProjectName; } }


        private void ConnectToTfs()
        {
            this.tfsConnectivity.ConnectUI();
            this.RaisePropertyChanged(() => this.TeamProjectCollectionUri);
            this.RaisePropertyChanged(() => this.TeamProjectName);
            this.RaisePropertyChanged(() => this.IsConnectedToTfs);
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
