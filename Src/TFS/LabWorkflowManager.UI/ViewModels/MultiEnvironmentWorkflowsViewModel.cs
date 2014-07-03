using _4tecture.UI.Common.Events;
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
using _4tecture.UI.Common.ViewModels;
using Microsoft.Practices.Prism.Events;

namespace LabWorkflowManager.UI.ViewModels
{
    public class MultiEnvironmentWorkflowsViewModel : NotificationObjectWithValidation
    {
        private ITFSConnectivity tfsConnectivity;
        private ITFSBuild tfsBuild;
        private ITFSLabEnvironment tfsLabEnvironment;
        private ITFSTest tfsTest;
        private IWorkflowManagerStorage workflowManagerStorage;
        private IRegionManager regionManager;
        private IFileDialogService fileDialogService;
        private IEventAggregator eventAggregator;

        public MultiEnvironmentWorkflowsViewModel(IWorkflowManagerStorage workflowManagerStorage, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager, IFileDialogService fileDialogService, IEventAggregator eventAggregator)
        {
            this.workflowManagerStorage = workflowManagerStorage;
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            this.regionManager = regionManager;
            this.fileDialogService = fileDialogService;
            this.eventAggregator = eventAggregator;

            this.tfsConnectivity.PropertyChanged += (source, args) => { if (args.PropertyName.Equals("IsConnected")) { this.RaisePropertyChanged(() => this.IsConnectedToTfs); this.RaisePropertyChanged(() => this.CanEditDefinitions); } };
            this.tfsConnectivity.PropertyChanged += (source, args) => { if (args.PropertyName.Equals("TfsUri")) { this.RaisePropertyChanged(() => this.TeamProjectCollectionUri); } };
            this.tfsConnectivity.PropertyChanged += (source, args) => { if (args.PropertyName.Equals("TeamProjectName")) { this.RaisePropertyChanged(() => this.TeamProjectName); } };

            this.workflowManagerStorage.PropertyChanged += (source, args) => { if (args.PropertyName.Equals("Definitions")) { this.RaisePropertyChanged(() => this.Definitions); this.RaisePropertyChanged(() => this.CanEditDefinitions); } };
            this.workflowManagerStorage.PropertyChanged += (source, args) => { if (args.PropertyName.Equals("CurrentDefinitionFile")) { this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } };

            if (!string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile))
            {
                this.workflowManagerStorage.Load(this.CurrentWorkflowDefinitionFile);
                this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile);
            }

            this.ConnectToTfsCommand = new DelegateCommand(ConnectToTfs, () => !this.tfsConnectivity.IsConnecting);
            this.AddNewDefinitionCommand = new DelegateCommand(AddNewDefinition);
            this.EditDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(EditDefinition);
            this.NewCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.SaveFile(out filepath)) { this.workflowManagerStorage.New(filepath); this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } });
            this.LoadCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.OpenFile(out filepath)) { this.workflowManagerStorage.Load(filepath); this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } });
            this.SaveCommand = new DelegateCommand(() => this.workflowManagerStorage.Save(this.CurrentWorkflowDefinitionFile), () => !string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile));
            this.SaveAsCommand = new DelegateCommand(() => { string filepath; if (this.fileDialogService.SaveFile(out filepath)) { this.workflowManagerStorage.Save(filepath); this.RaisePropertyChanged(() => this.CurrentWorkflowDefinitionFile); } }, () => !string.IsNullOrWhiteSpace(this.CurrentWorkflowDefinitionFile));
            this.DeleteDefinitionCommand = new DelegateCommand<MultiEnvironmentWorkflowDefinition>(DeleteDefinition);

            this.tfsConnectivity.PropertyChanged += (source, args) =>
            {
                if (args.PropertyName.Equals("IsConnecting"))
                {
                    ((DelegateCommand)this.ConnectToTfsCommand).RaiseCanExecuteChanged();
                    this.RaisePropertyChanged(() => this.IsConnecting);
                }
                else if (args.PropertyName.Equals("IsConnected"))
                {
                    this.RaisePropertyChanged(() => this.IsConnectedToTfs);
                    this.RaisePropertyChanged(() => this.TeamProjectCollectionUri);
                    this.RaisePropertyChanged(() => this.TeamProjectName);
                }
            };

            if (this.workflowManagerStorage.LastTFSConnection != null)
            {
                this.tfsConnectivity.Connect(this.workflowManagerStorage.LastTFSConnection.Uri, this.workflowManagerStorage.LastTFSConnection.Project);
            }

            this.eventAggregator.GetEvent<ApplicationClosingInterceptorEvent>().Subscribe(this.HandleApplicationClosing);
        }

        private void HandleApplicationClosing(System.ComponentModel.CancelEventArgs obj)
        {
            if (obj.Cancel == false)
            {
                if (this.Definitions.Any(o => o.IsDirty))
                {
                    var questionArgs = new ShowQuestionMessageArgs(){ Msg = ModuleStrings.MsgUnsavedChangesExit};
                    this.eventAggregator.GetEvent<ShowQuestionMessageEvent>().Publish(questionArgs);

                    if (questionArgs.Result == MessageResult.Yes)
                    {
                        obj.Cancel = false;
                    }
                    else
                    {
                        obj.Cancel = true;
                    }
                }
            }
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
            get { return this.tfsConnectivity.IsConnected; }
        }

        public bool IsConnecting
        {
            get { return this.tfsConnectivity.IsConnecting; }
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

        public bool CanEditDefinitions
        {
            get
            {
                return this.IsConnectedToTfs && this.Definitions != null;
            }
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
        }

        private void AddNewDefinition()
        {
            var newDefinition = new MultiEnvironmentWorkflowDefinition() { Name = ModuleStrings.NewDefinitionname };
            this.workflowManagerStorage.Definitions.Add(newDefinition);
            this.CurrentDefinition = newDefinition;
        }

        private void EditDefinition(MultiEnvironmentWorkflowDefinition item)
        {
            var existingViewModel = this.regionManager.Regions[RegionNames.MainRegion].Views.Where(v =>
            {
                var mewdvm = v as MultiEnvironmentWorkflowDefinitionViewModel;
                if (mewdvm != null)
                {
                    return mewdvm.Item.Equals(item);
                }
                return false;
            }).FirstOrDefault();

            if (existingViewModel != null)
            {
                this.regionManager.Regions[RegionNames.MainRegion].Activate(existingViewModel);
            }
            else
            {
                var vm = new MultiEnvironmentWorkflowDefinitionViewModel(item, this.tfsConnectivity, this.tfsBuild, this.tfsLabEnvironment, this.tfsTest, this.regionManager, this.eventAggregator);
                vm.CloseViewCommand = new DelegateCommand(() => { this.regionManager.Regions[RegionNames.MainRegion].Remove(vm); });
                this.regionManager.AddToRegion(RegionNames.MainRegion, vm);
            }
        }

        private void DeleteDefinition(MultiEnvironmentWorkflowDefinition item)
        {
            this.workflowManagerStorage.Definitions.Remove(item);
            this.RaisePropertyChanged(() => this.Definitions);
        }
    }
}
