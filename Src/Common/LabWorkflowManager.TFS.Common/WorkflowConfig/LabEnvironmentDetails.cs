using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class LabEnvironmentDetails : NotificationObject
    {
        private string labEnvironmentUri;
        private string snapshotName;
        public string LabEnvironmentUri { get { return this.labEnvironmentUri; } set { this.labEnvironmentUri = value; this.RaisePropertyChanged(() => this.LabEnvironmentUri); } }
        public string SnapshotName { get { return this.snapshotName; } set { this.snapshotName = value; this.RaisePropertyChanged(() => this.SnapshotName); } }

        private bool revertToSnapshot;
        public bool RevertToSnapthot { get { return this.revertToSnapshot; } 
            set { this.revertToSnapshot = value; this.RaisePropertyChanged(() => this.RevertToSnapthot); } }

        internal LabEnvironmentDetails Clone()
        {
            var clone = new LabEnvironmentDetails();

            clone.LabEnvironmentUri = this.LabEnvironmentUri;
            clone.SnapshotName = this.SnapshotName;
            clone.RevertToSnapthot = this.RevertToSnapthot;

            return clone;
        }

        
    }
}
