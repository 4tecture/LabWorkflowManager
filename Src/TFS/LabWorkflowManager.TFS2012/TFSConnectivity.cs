using _4tecture.UI.Common.Events;
using LabWorkflowManager.TFS.Common;
using Microsoft.Practices.Prism.Events;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2012
{
    public class TFSConnectivity : ITFSConnectivity
    {
        private IWorkflowManagerStorage workflowManagerStorage;
        private IEventAggregator eventAggregator;
        public TFSConnectivity(IWorkflowManagerStorage storage, IEventAggregator eventAggregator)
        {
            this.workflowManagerStorage = storage;
            this.eventAggregator = eventAggregator;
        }
        public void ConnectUI()
        {
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
                }
            }
            catch(Exception ex)
            {
                this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotConnectTFS");
            }
        }

        public void Connect(string tfsUri, string projectName)
        {
            try { 
            var tpc = new TfsTeamProjectCollection(new Uri(tfsUri));
            if (tpc != null)
            {
                Tpc = tpc;
                Tpc.EnsureAuthenticated();
                TeamProjects = tpc.GetService<ICommonStructureService>().ListAllProjects().Where(p => p.Name.Contains(projectName)).Take(1);
                this.workflowManagerStorage.LastTFSConnection = new TFSConnectionDetails() { Uri = Tpc.Uri.ToString(), Project = TeamProjects.First().Name };
            }
            }
            catch (Exception ex)
            {
                this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotConnectTFS");
            }
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
    }
}
