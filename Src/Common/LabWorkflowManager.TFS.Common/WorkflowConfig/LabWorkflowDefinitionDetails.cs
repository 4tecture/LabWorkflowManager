using _4tecture.UI.Common.ViewModels;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class LabWorkflowDefinitionDetails: NotificationObjectWithValidation
    {
        private LabBuildDefinitionDetails labBuildDefinitionDetails;
        public LabBuildDefinitionDetails LabBuildDefinitionDetails { get { return this.labBuildDefinitionDetails; } set { this.labBuildDefinitionDetails = value; this.RaisePropertyChanged(() => this.LabBuildDefinitionDetails); } }

        private SourceBuildDetails sourceBuildDetails;
        public SourceBuildDetails SourceBuildDetails { get { return this.sourceBuildDetails; } set { this.sourceBuildDetails = value; this.RaisePropertyChanged(() => this.SourceBuildDetails); } }

        private LabEnvironmentDetails labEnvironmentDetails;
        public LabEnvironmentDetails LabEnvironmentDetails { get { return this.labEnvironmentDetails; } set { this.labEnvironmentDetails = value; this.RaisePropertyChanged(() => this.LabEnvironmentDetails); } }

        private DeploymentDetails deploymentDetails;
        public DeploymentDetails DeploymentDetails { get { return this.deploymentDetails; } set { this.deploymentDetails = value; this.RaisePropertyChanged(() => this.DeploymentDetails); } }

        private TestDetails testDetails;
        public TestDetails TestDetails { get { return this.testDetails; } set { this.testDetails = value; this.RaisePropertyChanged(() => this.TestDetails); } }

        internal LabWorkflowDefinitionDetails Clone()
        {
            var clone = new LabWorkflowDefinitionDetails();
            
            clone.LabBuildDefinitionDetails = this.LabBuildDefinitionDetails.Clone();
            clone.SourceBuildDetails = this.SourceBuildDetails.Clone();
            clone.LabEnvironmentDetails = this.LabEnvironmentDetails.Clone();
            clone.DeploymentDetails = this.DeploymentDetails.Clone();
            clone.TestDetails = this.TestDetails.Clone();

            return clone;
        }

        public override bool HasErrors
        {
            get
            {
                return this.HasErrorsInternal ||
                    this.LabBuildDefinitionDetails.HasErrors ||
                       this.SourceBuildDetails.HasErrors ||
                       this.LabEnvironmentDetails.HasErrors ||
                       this.DeploymentDetails.HasErrors ||
                       this.TestDetails.HasErrors;
            }
        }
    }
}
