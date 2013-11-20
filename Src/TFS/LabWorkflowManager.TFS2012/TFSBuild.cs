using LabWorkflowManager.TFS.Common;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.Lab.Workflow.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2012
{
    public class TFSBuild
    {
        public TFSBuild()
        {
            this.Connectivity = new TFSConnectivity();
        }
        private TFSConnectivity connectivity;
        public ITFSConnectivity Connectivity { get{return this.connectivity;} private set{this.connectivity = value as TFSConnectivity;} }

        public IBuildServer BuildServer { get
        {
             if (this.Connectivity.IsConnected)
            {
                return this.connectivity.Tpc.GetService<IBuildServer>();
            }
            return null;
            }
        }

        public IEnumerable<IBuildDefinition> QueryBuildDefinitions()
        {
            if (this.BuildServer != null)
            {
                var buildDefinitions = this.BuildServer.QueryBuildDefinitions(this.connectivity.TeamProjects.First().Name);
                return buildDefinitions;
            }
            return new List<IBuildDefinition>();
        }

        public IBuildDefinition CreateBuildDefinition(string buildDefinitionName, string buildDefinitionDescription, string buildControllerName, string processTemplateFilename, LabWorkflowManager.TFS.Common.SourceBuildDetails sourceBuildDetails, LabWorkflowManager.TFS.Common.EnvironmentDetails labEnvironmentDetails, LabWorkflowManager.TFS.Common.DeploymentDetails deploymentDetails, LabWorkflowManager.TFS.Common.TestDetails testDetails)
        {
            if(this.BuildServer != null)
            {
                var buildDefinition = this.BuildServer.CreateBuildDefinition(this.connectivity.TeamProjects.First().Name);

                buildDefinition.Name = buildDefinitionName;
                buildDefinition.Description = buildDefinitionDescription;
                //buildDefinition.ContinuousIntegrationType = buildDefinitionConinuousIntegrationType;
                buildDefinition.BuildController = this.BuildServer.GetBuildController(string.Format("*{0}*", buildControllerName));

                var buildDefinitionProcessTemplate = this.BuildServer.QueryProcessTemplates(this.connectivity.TeamProjects.First().Name).Where(t => t.ServerPath.Contains(processTemplateFilename)).First();
                buildDefinition.Process = buildDefinitionProcessTemplate;
                var process = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);

                var labWorkflowDetails = new LabWorkflowDetails();
                if (sourceBuildDetails.BuildDefinitionUri != null)
                {
                    var compileBuildDefinition = this.BuildServer.GetBuildDefinition(sourceBuildDetails.BuildDefinitionUri);
                    labWorkflowDetails.BuildDetails.BuildDefinitionUri = compileBuildDefinition.Uri;
                    labWorkflowDetails.BuildDetails.BuildDefinitionName = compileBuildDefinition.Name;
                    labWorkflowDetails.BuildDetails.BuildUri = sourceBuildDetails.BuildUri;
                    labWorkflowDetails.BuildDetails.IsTeamSystemBuild = true;
                }
                else
                {
                    labWorkflowDetails.BuildDetails.CustomBuildPath = sourceBuildDetails.CustomBuildPath;
                }

                var labService = this.connectivity.Tpc.GetService<LabService>();
                var environment = labService.GetLabEnvironment(labEnvironmentDetails.LabEnvironmentUri);
                labWorkflowDetails.EnvironmentDetails.LabEnvironmentUri = environment.Uri;
                labWorkflowDetails.EnvironmentDetails.LabEnvironmentName = environment.Name;
                if(!string.IsNullOrWhiteSpace(labEnvironmentDetails.SnapshotName))
                {
                    labWorkflowDetails.EnvironmentDetails.SnapshotName = labEnvironmentDetails.SnapshotName;
                    labWorkflowDetails.EnvironmentDetails.RevertToSnapshot = true;
                }


                labWorkflowDetails.DeploymentDetails.DeploymentNeeded = true;
                labWorkflowDetails.DeploymentDetails.UseRoleForDeployment = true;
                labWorkflowDetails.DeploymentDetails.TakePostDeploymentSnapshot = true;
                labWorkflowDetails.DeploymentDetails.PostDeploymentSnapshotName = deploymentDetails.SnapshotName;
                labWorkflowDetails.DeploymentDetails.UseRoleForDeployment = true;
                labWorkflowDetails.DeploymentDetails.Scripts = new Microsoft.TeamFoundation.Build.Workflow.Activities.StringList();
                labWorkflowDetails.DeploymentDetails.Scripts.AddRange(deploymentDetails.ScriptStrings);

                labWorkflowDetails.TestParameters.ProjectName = this.connectivity.TeamProjects.First().Name;
                labWorkflowDetails.TestParameters.RunTest = true;
                labWorkflowDetails.TestParameters.TestPlanId = testDetails.TestPlanId;
                labWorkflowDetails.TestParameters.TestSuiteIdList = testDetails.TestSuiteIdList;
                labWorkflowDetails.TestParameters.TestConfigurationId = testDetails.TestConfigurationId;


                process.Add("LabWorkflowParameters", labWorkflowDetails);
                
                buildDefinition.ProcessParameters = WorkflowHelpers.SerializeProcessParameters(process);
                buildDefinition.Save();
                return buildDefinition;

            }
            throw new Exception("No connection to TFS!");
        }
    }
}
