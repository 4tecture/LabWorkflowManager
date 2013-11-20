using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.Lab.Workflow.Activities;

namespace LabWorkflowManager.TFS2012.Tests.Integration
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tfsbuild = new TFSBuild(new TFSConnectivity());
            tfsbuild.Connectivity.Connect("http://vsalm:8080/tfs/FabrikamFiberCollection", "FabrikamFiber");

            var buildDefinitions = tfsbuild.QueryBuildDefinitions();

            foreach (var bd in buildDefinitions)
            {
                if (bd.Name.Contains("BDT"))
                {
                    var process = WorkflowHelpers.DeserializeProcessParameters(bd.ProcessParameters);
                    if (process.ContainsKey("LabWorkflowParameters"))
                    {
                        var param = process["LabWorkflowParameters"];
                        if (param != null)
                        {
                            var lwd = param as LabWorkflowDetails;
                            if (lwd != null)
                            {
                                var buildDefinitionName = lwd.BuildDetails.BuildDefinitionName;
                                var buildDefinitionUri = lwd.BuildDetails.BuildDefinitionUri;
                                var buildUri = lwd.BuildDetails.BuildUri;
                                var configuration = lwd.BuildDetails.Configuration;
                                var customBuildPath = lwd.BuildDetails.CustomBuildPath;
                                var isTeamSystemBuild = lwd.BuildDetails.IsTeamSystemBuild;
                                var queueNewBuild = lwd.BuildDetails.QueueNewBuild;

                                var disposition = lwd.EnvironmentDetails.Disposition;
                                var HostGroupName = lwd.EnvironmentDetails.HostGroupName;
                                var LabEnvironmentName = lwd.EnvironmentDetails.LabEnvironmentName;
                                var LabEnvironmentUri = lwd.EnvironmentDetails.LabEnvironmentUri;
                                var LabLibraryShareName = lwd.EnvironmentDetails.LabLibraryShareName;
                                var NewLabEnvironmentName = lwd.EnvironmentDetails.NewLabEnvironmentName;
                                var ProjectName = lwd.EnvironmentDetails.ProjectName;
                                var RevertToSnapshot = lwd.EnvironmentDetails.RevertToSnapshot;
                                var SnapshotName = lwd.EnvironmentDetails.SnapshotName;
                                var TfsUrl = lwd.EnvironmentDetails.TfsUrl;

                                var scripts = lwd.DeploymentDetails.Scripts;

                                var planid = lwd.TestParameters.TestPlanId;
                                var testsuitelist = lwd.TestParameters.TestSuiteIdList;
                                var configurationdi = lwd.TestParameters.TestConfigurationId;
                            }
                        }
                    }
                }
            }

        }
    }
}
