using LabWorkflowManager.TFS.Common;
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
        public void ConnectUI()
        {
            TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            tpp.ShowDialog();
            if(tpp.SelectedTeamProjectCollection != null)
            {
                Tpc = tpp.SelectedTeamProjectCollection;
                Tpc.EnsureAuthenticated();
                TeamProjects = tpp.SelectedProjects;
            }
        }

        public void Connect(string tfsUri, string projectName)
        {
            var tpc = new TfsTeamProjectCollection(new Uri(tfsUri));
            if (tpc != null)
            {
                Tpc = tpc;
                Tpc.EnsureAuthenticated();
                TeamProjects = tpc.GetService<ICommonStructureService>().ListAllProjects().Where(p => p.Name.Contains(projectName)).Take(1);
            }
        }

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
