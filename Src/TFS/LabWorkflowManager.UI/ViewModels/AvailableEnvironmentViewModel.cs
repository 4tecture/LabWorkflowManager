using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using _4tecture.UI.Common.Helper;
using _4tecture.UI.Common.ViewModels;

namespace LabWorkflowManager.UI.ViewModels
{
    public class AvailableEnvironmentViewModel : NotificationObjectWithValidation
    {
        public AvailableEnvironmentViewModel(AssociatedLabEnvironment associatedLabEnvironment)
        {
            this.AssociatedLabEnvironment = associatedLabEnvironment;
        }

        private TFS.Common.WorkflowConfig.AssociatedLabEnvironment associatedLabEnvironment;
        public AssociatedLabEnvironment AssociatedLabEnvironment
        {
            get { return this.associatedLabEnvironment; }
            private set { this.associatedLabEnvironment = value; this.RaisePropertyChanged(() => this.AssociatedLabEnvironment); }
        }
        public string Uri { 
            get { return this.AssociatedLabEnvironment.Uri; } 
            set{this.AssociatedLabEnvironment.Uri = value;this.RaisePropertyChanged(() => this.Uri);}
        }


        public List<string> Snapshots
        {
            get { return this.AssociatedLabEnvironment.Snapshots; }
            set { this.AssociatedLabEnvironment.Snapshots = value; this.RaisePropertyChanged(() => this.Snapshots); }
        }

        public string Name
        {
            get { return this.AssociatedLabEnvironment.Name; }
            set { this.AssociatedLabEnvironment.Name = value; this.RaisePropertyChanged(() => this.Name); }
        }

        public List<string> Roles
        {
            get { return this.AssociatedLabEnvironment.Roles; }
            set { this.AssociatedLabEnvironment.Roles = value; this.RaisePropertyChanged(() => this.Roles); }
        }

        public override bool Equals(object obj)
        {
            var other = obj as AvailableEnvironmentViewModel;
            if (other == null)
            {
                return false;
            }
            return this.Uri.Equals(other.Uri);
        }

        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        private SelectableCollection<AssociatedTestConfiguration> availableTestConfigurations;
        
        public SelectableCollection<AssociatedTestConfiguration> AvailableTestConfigurations
        {
            get { return this.availableTestConfigurations; }
            set { this.availableTestConfigurations = value; this.RaisePropertyChanged(() => this.AvailableTestConfigurations); }
        }
    }
}
