using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4tecture.UI.Common.ViewModels;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class DeploymentDetails : NotificationObjectWithValidation
    {
        private string snapshotName;
        private ObservableCollection<DeploymentScript> scripts;
        public DeploymentDetails()
        {
            this.Scripts = new ObservableCollection<DeploymentScript>();
        }
        public string SnapshotName { get { return this.snapshotName; } set { this.snapshotName = value; this.RaisePropertyChanged(() => this.SnapshotName); } }

        public ObservableCollection<DeploymentScript> Scripts { get { return this.scripts; } set { this.scripts = value; this.RaisePropertyChanged(() => this.Scripts); } }

        private bool takePostDeploymentSnapshot;
        public bool TakePostDeploymentSnapshot
        {
            get { return this.takePostDeploymentSnapshot; }
            set { this.takePostDeploymentSnapshot = value; this.RaisePropertyChanged(() => this.TakePostDeploymentSnapshot); }
        }

        public IEnumerable<string> ScriptStrings { get { return Scripts.Select(p => p.ToString()); } }

        internal DeploymentDetails Clone()
        {
            var clone = new DeploymentDetails();

            clone.SnapshotName = this.SnapshotName;
            clone.TakePostDeploymentSnapshot = this.TakePostDeploymentSnapshot;

            foreach (var scriptItem in this.Scripts)
            {
                clone.Scripts.Add(new DeploymentScript() { Role = scriptItem.Role, Script = scriptItem.Script, WorkingDirectory = scriptItem.WorkingDirectory });
            }

            return clone;
        }


    }

    public class DeploymentScript : NotificationObject
    {
        private string role;
        private string script;
        private string workingDirectory;
        public string Role
        {
            get { return this.role; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) // todo deselection problem when tab change
                {
                    this.role = value; this.RaisePropertyChanged(() => this.Role);
                }
            }
        }
        public string Script { get { return this.script; } set { this.script = value; this.RaisePropertyChanged(() => this.Script); } }
        public string WorkingDirectory { get { return this.workingDirectory; } set { this.workingDirectory = value; this.RaisePropertyChanged(() => this.WorkingDirectory); } }

        public override string ToString()
        {
            return string.Join(" | ", Role, Script, WorkingDirectory);
        }
    }
}
