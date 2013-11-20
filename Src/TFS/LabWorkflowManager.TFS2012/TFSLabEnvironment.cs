using LabWorkflowManager.TFS.Common;
using Microsoft.TeamFoundation.Lab.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2012
{
    public class TFSLabEnvironment : ITFSLabEnvironment
    {
        public TFSLabEnvironment(ITFSConnectivity connectivity)
        {
            this.Connectivity = connectivity as TFSConnectivity;
        }

        private TFSConnectivity connectivity;
        public ITFSConnectivity Connectivity { get { return this.connectivity; } private set { this.connectivity = value as TFSConnectivity; } }

        public LabService LabService
        {
            get
            {
                if (this.Connectivity.IsConnected)
                {
                    return this.connectivity.Tpc.GetService<LabService>();
                }
                return null;
            }
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedLabEnvironment> GetAssociatedLabEnvironments()
        {
            foreach(var env in this.QueryEnvironments())
            {
                yield return this.ConvertLabEnvironment(env);
            }
        }

        private TFS.Common.WorkflowConfig.AssociatedLabEnvironment ConvertLabEnvironment(LabEnvironment env)
        {
            var ale = new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedLabEnvironment();
            ale.Uri = env.Uri;
            ale.Name = env.Name;
            ale.Snapshots = env.QueryLabEnvironmentSnapshots().Select(o => o.Name).ToList();

            return ale;
        }

        public IEnumerable<LabEnvironment> QueryEnvironments()
        {
            if(this.LabService != null)
            {
                var environments = this.LabService.QueryLabEnvironments(new LabEnvironmentQuerySpec() { Project = this.connectivity.TeamProjects.First().Name });
            }
            return new List<LabEnvironment>();
        }

        public void ChangeEnvironmentOwner(Uri labEnvironmentUri, string newOwner)
        {
            if (this.LabService != null)
            {
                var env = this.LabService.GetLabEnvironment(labEnvironmentUri);
                var updatePack = new LabEnvironmentUpdatePack();
                foreach (var labsystem in env.LabSystems)
                {
                    updatePack.ListOfUpdateCommands.Add(new UpdateLabSystemCommand(labsystem) { LabSystemUri = labsystem.Uri, VMOwner = newOwner });
                }

                this.LabService.UpdateLabEnvironment(labEnvironmentUri, updatePack);
            }
        }
    }
}
