using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
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
        public MultiEnvironmentWorkflowsViewModel(ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest)
        {
            this.tfsConnectivity = tfsConnectivity;
            this.tfsBuild = tfsBuild;
            this.tfsLabEnvironment = tfsLabEnvironment;
            this.tfsTest = tfsTest;
            
            this.ConnectToTfsCommand = new DelegateCommand(ConnectToTfs);
        }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.NavigationTitle;
            }
        }


        public ICommand ConnectToTfsCommand { get; private set; }

        public string TeamProjectCollectionUri { get { return this.tfsConnectivity.TfsUri; } }
        public string TeamProjectName { get { return this.tfsConnectivity.TeamProjectName; } }


        private void ConnectToTfs()
        {
            this.tfsConnectivity.ConnectUI();
            this.RaisePropertyChanged(() => this.TeamProjectCollectionUri);
            this.RaisePropertyChanged(() => this.TeamProjectName);
        }
    }
}
