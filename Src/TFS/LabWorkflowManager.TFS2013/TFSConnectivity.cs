using _4tecture.UI.Common.Events;
using LabWorkflowManager.TFS.Common;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2013
{
    public class TFSConnectivity : NotificationObject, ITFSConnectivity
    {
        private IWorkflowManagerStorage workflowManagerStorage;
        private IEventAggregator eventAggregator;
        public TFSConnectivity(IWorkflowManagerStorage storage, IEventAggregator eventAggregator)
        {
            this.workflowManagerStorage = storage;
            this.eventAggregator = eventAggregator;
        }
        public async void ConnectUI()
        {
            await Task.Run(() =>
            {
                this.IsConnecting = true;
                try
                {
                    TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
                    tpp.ShowDialog();
                    if (tpp.SelectedTeamProjectCollection != null)
                    {
                        Tpc = tpp.SelectedTeamProjectCollection;
                        Tpc.EnsureAuthenticated();
                        TeamProjects = tpp.SelectedProjects;
                        this.workflowManagerStorage.LastTFSConnection = new TFSConnectionDetails() { Uri = Tpc.Uri.ToString(), Project = TeamProjects.First().Name };
                        this.RaisePropertyChanged(() => this.IsConnected);
                    }
                }
                catch (Exception ex)
                {
                    this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotConnectTFS");
                }
                this.IsConnecting = false;
            });
        }

        public async void Connect(string tfsUri, string projectName)
        {
            await Task.Run(() =>
            {
                this.IsConnecting = true;
                try
                {
                    var tpc = new TfsTeamProjectCollection(new Uri(tfsUri));
                    if (tpc != null)
                    {
                        Tpc = tpc;
                        Tpc.EnsureAuthenticated();
                        TeamProjects = tpc.GetService<ICommonStructureService>().ListAllProjects().Where(p => p.Name.Contains(projectName)).Take(1);
                        this.workflowManagerStorage.LastTFSConnection = new TFSConnectionDetails() { Uri = Tpc.Uri.ToString(), Project = TeamProjects.First().Name };
                        this.RaisePropertyChanged(() => this.IsConnected);
                    }
                }
                catch (Exception ex)
                {
                    this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotConnectTFS");
                }
                this.IsConnecting = false;
            });
        }

        public string TfsUri { get { return this.Tpc != null ? this.Tpc.Uri.ToString() : string.Empty; } }
        public string TeamProjectName { get { return this.TeamProjects != null && this.TeamProjects.Count() > 0 ? this.TeamProjects.First().Name : string.Empty; } }

        public TfsTeamProjectCollection Tpc { get; set; }

        public IEnumerable<Microsoft.TeamFoundation.Server.ProjectInfo> TeamProjects { get; set; }

        public bool IsConnected
        {
            get
            {
                return this.Tpc != null && this.TeamProjects != null && this.TeamProjects.Count() > 0;
            }
        }

        private bool isConnecting;

        public bool IsConnecting
        {
            get { return isConnecting; }
            set { isConnecting = value; this.RaisePropertyChanged(() => this.IsConnecting); }
        }

    }
}
