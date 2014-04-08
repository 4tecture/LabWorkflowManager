using LabWorkflowManager.TFS.Common.Resources;
using _4tecture.UI.Common.ViewModels;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class SourceBuildDetails:NotificationObjectWithValidation
    {
        private string buildDefinitionUri;

        public string BuildDefinitionUri
        {
            get { return buildDefinitionUri; }
            set { buildDefinitionUri = value; this.customBuildPath = null; VerifyBuildDefinitionSettings(); this.RaisePropertyChanged(() => this.BuildDefinitionUri); this.RaisePropertyChanged(() => this.CustomBuildPath); }
        }
        
        private string buildUri;

        public string BuildUri
        {
            get { return buildUri; }
            set { buildUri = value; this.RaisePropertyChanged(() => this.BuildUri); }
        }

        private bool queueNewBuild;

        public bool QueueNewBuild
        {
            get { return queueNewBuild; }
            set { queueNewBuild = value; this.RaisePropertyChanged(() => this.QueueNewBuild); }
        }



        private string customBuildPath;

        public string CustomBuildPath
        {
            get { return customBuildPath; }
            set { customBuildPath = value; this.buildDefinitionUri = null; this.buildUri = null; this.RaisePropertyChanged(() => this.BuildDefinitionUri); this.RaisePropertyChanged(() => this.CustomBuildPath); this.RaisePropertyChanged(() => this.BuildUri); }
        }


        internal SourceBuildDetails Clone()
        {
            var clone = new SourceBuildDetails();

            clone.buildDefinitionUri = this.buildDefinitionUri;
            clone.buildUri = this.buildUri;
            clone.customBuildPath = this.customBuildPath;

            return clone;
        }

        private void VerifyBuildDefinitionSettings()
        {
            if (string.IsNullOrWhiteSpace(this.BuildDefinitionUri) && string.IsNullOrWhiteSpace(this.CustomBuildPath))
            {
                this.AddError("BuildDefinitionUri", CommonStrings.ErrorSourcebuildDetailsBuildDefinitionOrCustomBuildPathNotSet);
                this.AddError("CustomBuildPath", CommonStrings.ErrorSourcebuildDetailsBuildDefinitionOrCustomBuildPathNotSet);
            }
            else
            {
                this.RemoveError("BuildDefinitionUri", CommonStrings.ErrorSourcebuildDetailsBuildDefinitionOrCustomBuildPathNotSet);
                this.RemoveError("CustomBuildPath", CommonStrings.ErrorSourcebuildDetailsBuildDefinitionOrCustomBuildPathNotSet);
            }
        }
    }
}
