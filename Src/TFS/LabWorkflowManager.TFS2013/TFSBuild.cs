﻿using System.Runtime.Remoting.Messaging;
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

namespace LabWorkflowManager.TFS2013
{
    public class TFSBuild : ITFSBuild
    {
        public TFSBuild(ITFSConnectivity connectivity)
        {
            this.Connectivity = connectivity as TFSConnectivity;
        }
        private TFSConnectivity connectivity;
        public ITFSConnectivity Connectivity { get { return this.connectivity; } private set { this.connectivity = value as TFSConnectivity; } }

        public IBuildServer BuildServer
        {
            get
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

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetMultiEnvAssociatedBuildDefinitions(Guid multiEnvConfigId)
        {
            var results = this.QueryBuildDefinitions().Where(o => !string.IsNullOrWhiteSpace(o.Description) && o.Description.Contains(string.Format("MultiEnvironmentWorkflowDefinition:{0}", multiEnvConfigId.ToString())));
            foreach (var res in results)
            {
                yield return ConvertToAssociatedBuildDefinition(res);
            }
        }

        public async Task DeleteMultiEnvAssociatedBuildDefinitions(Guid multiEnvConfigId)
        {
            await Task.Run(() =>
            {
                var existingBuildDefinitions = this.GetMultiEnvAssociatedBuildDefinitions(multiEnvConfigId).ToList();
                if (existingBuildDefinitions.Count > 0)
                {
                    this.DeleteBuildDefinition(existingBuildDefinitions.Select(o => new Uri(o.Uri)).ToArray());
                }
            });
        }

        public IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetAssociatedDefinitions()
        {
            var results = this.QueryBuildDefinitions();
            foreach (var res in results)
            {
                yield return ConvertToAssociatedBuildDefinition(res);
            }
        }

        private static TFS.Common.WorkflowConfig.AssociatedBuildDefinition ConvertToAssociatedBuildDefinition(IBuildDefinition res)
        {
            var abd = new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition();
            abd.Name = res.Name;
            abd.BuildControllerName = res.BuildController != null ? res.BuildController.Name : string.Empty;
            abd.BuildControllerUri = res.BuildControllerUri != null ? res.BuildControllerUri.ToString() : string.Empty;
            abd.ContinuousIntegrationQuietPeriod = res.ContinuousIntegrationQuietPeriod;
            abd.ContinuousIntegrationType = (LabWorkflowManager.TFS.Common.WorkflowConfig.BuildDefinitionContinuousIntegrationType)res.ContinuousIntegrationType;
            abd.DateCreated = res.DateCreated;
            abd.Description = res.Description;
            abd.Id = res.Id;
            abd.LastBuildUri = res.LastBuildUri != null ? res.LastBuildUri.ToString() : string.Empty;
            abd.LastGoodBuildLabel = res.LastGoodBuildLabel;
            abd.LastGoodBuildUri = res.LastGoodBuildUri != null ? res.LastGoodBuildUri.ToString() : string.Empty;
            abd.Builds = res.QueryBuilds().Select(o => new LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDetail() { Uri = o.Uri.ToString(), LabelName = o.LabelName }).ToList();
            abd.Uri = res.Uri.ToString();
            return abd;
        }

        public async Task<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> CreateBuildDefinitionFromDefinition(LabWorkflowManager.TFS.Common.WorkflowConfig.LabWorkflowDefinitionDetails labworkflowDefinitionDetails)
        {
            var def = await Task.Run( () => ConvertToAssociatedBuildDefinition(this.CreateBuildDefinition(labworkflowDefinitionDetails).Result));
            return await Task.Run(() => def);
        }

        public async void DeleteBuildDefinition(params Uri[] uris)
        {
            await Task.Run(() =>
            {
                foreach (var definitionUri in uris)
                {
                    try
                    {
                        var buildUris =
                            this.BuildServer.GetBuildDefinition(definitionUri).QueryBuilds().Select(o => o.Uri).ToArray();
                        this.BuildServer.DeleteBuilds(buildUris);
                    }
                    catch (BuildDefinitionNotFoundForUriException){} // exception if not builds available
                    this.BuildServer.DeleteBuildDefinitions(new Uri[]{definitionUri});
                }
            });
        }

        public async Task<IBuildDefinition> CreateBuildDefinition(LabWorkflowManager.TFS.Common.WorkflowConfig.LabWorkflowDefinitionDetails labworkflowDefinitionDetails)
        {
            return await Task.Run(() =>
            {
                if (this.BuildServer != null)
                {
                    var buildDefinition = this.QueryBuildDefinitions().FirstOrDefault(d => d.Name == labworkflowDefinitionDetails.LabBuildDefinitionDetails.Name);
                    if (buildDefinition == null)
                    {
                        buildDefinition = this.BuildServer.CreateBuildDefinition(this.connectivity.TeamProjects.First().Name);
                    }

                    ConfigMainBuildDefinitionSettings(labworkflowDefinitionDetails.LabBuildDefinitionDetails,
                        buildDefinition);

                    var processParameters =
                        WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);

                    var labWorkflowDetails = new LabWorkflowDetails();
                    ConfigLabBuildSettings(labworkflowDefinitionDetails.SourceBuildDetails, labWorkflowDetails);
                    ConfigLabEnvironmentSettings(labworkflowDefinitionDetails.LabEnvironmentDetails, labWorkflowDetails);
                    ConfigLabDeploymentSettings(labworkflowDefinitionDetails.DeploymentDetails, labWorkflowDetails);
                    ConfigLabTestSettings(labworkflowDefinitionDetails.TestDetails, labWorkflowDetails);

                    processParameters["LabWorkflowParameters"] = labWorkflowDetails;
                    buildDefinition.ProcessParameters = WorkflowHelpers.SerializeProcessParameters(processParameters);
                    buildDefinition.Save();
                    return buildDefinition;
                }
                throw new Exception("No connection to TFS!");
            });
        }


        private void ConfigLabTestSettings(LabWorkflowManager.TFS.Common.WorkflowConfig.TestDetails testDetails, LabWorkflowDetails labWorkflowDetails)
        {
            labWorkflowDetails.TestParameters.ProjectName = this.connectivity.TeamProjects.First().Name;
            labWorkflowDetails.TestParameters.RunTest = true;
            labWorkflowDetails.TestParameters.TestPlanId = testDetails.TestPlanId;
            labWorkflowDetails.TestParameters.TestSuiteIdList = testDetails.TestSuiteIdList.ToList();
            labWorkflowDetails.TestParameters.TestSettingsId = testDetails.TestSettingsId;
            labWorkflowDetails.TestParameters.TestConfigurationId = testDetails.TestConfigurationId;
        }

        private static void ConfigLabDeploymentSettings(LabWorkflowManager.TFS.Common.WorkflowConfig.DeploymentDetails deploymentDetails, LabWorkflowDetails labWorkflowDetails)
        {
            labWorkflowDetails.DeploymentDetails.DeploymentNeeded = true;
            labWorkflowDetails.DeploymentDetails.UseRoleForDeployment = true;
            labWorkflowDetails.DeploymentDetails.TakePostDeploymentSnapshot = deploymentDetails.TakePostDeploymentSnapshot;
            labWorkflowDetails.DeploymentDetails.PostDeploymentSnapshotName = deploymentDetails.SnapshotName;
            labWorkflowDetails.DeploymentDetails.UseRoleForDeployment = true;
            labWorkflowDetails.DeploymentDetails.Scripts = new Microsoft.TeamFoundation.Build.Workflow.Activities.StringList();
            labWorkflowDetails.DeploymentDetails.Scripts.AddRange(deploymentDetails.ScriptStrings);
        }

        private void ConfigLabEnvironmentSettings(LabWorkflowManager.TFS.Common.WorkflowConfig.LabEnvironmentDetails labEnvironmentDetails, LabWorkflowDetails labWorkflowDetails)
        {
            var labService = this.connectivity.Tpc.GetService<LabService>();
            var environment = labService.GetLabEnvironment(new Uri(labEnvironmentDetails.LabEnvironmentUri));
            labWorkflowDetails.EnvironmentDetails.LabEnvironmentUri = environment.Uri;
            labWorkflowDetails.EnvironmentDetails.LabEnvironmentName = environment.Name;
            if (!string.IsNullOrWhiteSpace(labEnvironmentDetails.SnapshotName))
            {
                labWorkflowDetails.EnvironmentDetails.SnapshotName = labEnvironmentDetails.SnapshotName;
                labWorkflowDetails.EnvironmentDetails.RevertToSnapshot = true;
            }

            labWorkflowDetails.EnvironmentDetails.RevertToSnapshot = labEnvironmentDetails.RevertToSnapshot;
        }

        private LabWorkflowDetails ConfigLabBuildSettings(LabWorkflowManager.TFS.Common.WorkflowConfig.SourceBuildDetails sourceBuildDetails, LabWorkflowDetails labWorkflowDetails)
        {
            if (sourceBuildDetails.BuildDefinitionUri != null)
            {
                var compileBuildDefinition = this.BuildServer.GetBuildDefinition(new Uri(sourceBuildDetails.BuildDefinitionUri));
                labWorkflowDetails.BuildDetails.BuildDefinitionUri = compileBuildDefinition.Uri;
                labWorkflowDetails.BuildDetails.BuildDefinitionName = compileBuildDefinition.Name;
                labWorkflowDetails.BuildDetails.QueueNewBuild = sourceBuildDetails.QueueNewBuild;
                if (!string.IsNullOrWhiteSpace(sourceBuildDetails.BuildUri))
                {
                    labWorkflowDetails.BuildDetails.BuildUri = new Uri(sourceBuildDetails.BuildUri);
                }
                labWorkflowDetails.BuildDetails.IsTeamSystemBuild = true;
            }
            else
            {
                labWorkflowDetails.BuildDetails.CustomBuildPath = sourceBuildDetails.CustomBuildPath;
            }
            return labWorkflowDetails;
        }

        private void ConfigMainBuildDefinitionSettings(LabWorkflowManager.TFS.Common.WorkflowConfig.LabBuildDefinitionDetails buildDefinitionDetails, IBuildDefinition buildDefinition)
        {
            buildDefinition.Name = buildDefinitionDetails.Name;
            buildDefinition.Description = buildDefinitionDetails.Description;
            buildDefinition.ContinuousIntegrationType = (ContinuousIntegrationType)buildDefinitionDetails.ContinuousIntegrationType;
            if (buildDefinition.ContinuousIntegrationType == ContinuousIntegrationType.Schedule || buildDefinition.ContinuousIntegrationType == ContinuousIntegrationType.ScheduleForced)
            {
                buildDefinition.Schedules.Clear();
                var schedule = buildDefinition.AddSchedule();
                schedule.DaysToBuild = (ScheduleDays)buildDefinitionDetails.ScheduledDays;
                schedule.StartTime = buildDefinitionDetails.StartTime;
            }
            else if (buildDefinition.ContinuousIntegrationType == ContinuousIntegrationType.Batch)
            {
                buildDefinition.ContinuousIntegrationQuietPeriod = buildDefinitionDetails.QuietPeriod;
            }
            buildDefinition.BuildController = this.BuildServer.GetBuildController(string.Format("*{0}*", buildDefinitionDetails.ControllerName));

            var buildDefinitionProcessTemplate = this.BuildServer.QueryProcessTemplates(this.connectivity.TeamProjects.First().Name).Where(t => t.ServerPath.Contains(buildDefinitionDetails.ProcessTemplateFilename)).First();
            buildDefinition.Process = buildDefinitionProcessTemplate;
        }

        public IEnumerable<string> GetProcessTemplateFiles()
        {
            return this.BuildServer.QueryProcessTemplates(this.connectivity.TeamProjects.First().Name).Select(o => o.ServerPath.Substring(o.ServerPath.LastIndexOf("/") + 1));
        }

        public IEnumerable<string> GetBuildControllers()
        {
            return this.BuildServer.QueryBuildControllers().Select(o => o.Name);
        }
    }
}
