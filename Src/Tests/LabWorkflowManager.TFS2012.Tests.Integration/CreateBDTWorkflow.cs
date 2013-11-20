using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using System.Linq;

namespace LabWorkflowManager.TFS2012.Tests.Integration
{
    [TestClass]
    public class CreateBDTWorkflow
    {
        [TestMethod]
        public void CreateBDTWorkflowFromScratch()
        {
            // Arrange
            var tfsbuild = new TFSBuild(new TFSConnectivity());
            tfsbuild.Connectivity.Connect("http://vsalm:8080/tfs/FabrikamFiberCollection", "FabrikamFiber");

            var name = string.Format("Test BDT Integration {0}", DateTime.Now.Ticks);
            var description = string.Format("Description for Build Definition created by Integration Test {0}", DateTime.Now.Ticks);

            // Act
            tfsbuild.CreateBuildDefinition(new LabWorkflowManager.TFS.Common.WorkflowConfig.LabWorkflowDefinitionDetails(){
                LabBuildDefinitionDetails =  new LabWorkflowManager.TFS.Common.WorkflowConfig.LabBuildDefinitionDetails() { Name = name, Description = description, ControllerName = "VSALM", ProcessTemplateFilename = "LabDefaultTemplate.11.xaml" },
                SourceBuildDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.SourceBuildDetails() { BuildDefinitionUri = new Uri("vstfs:///Build/Definition/1") },
                LabEnvironmentDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.LabEnvironmentDetails() { LabEnvironmentUri = new Uri("vstfs:///LabManagement/LabEnvironment/2") },
                DeploymentDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.DeploymentDetails() { Scripts = new System.Collections.Generic.List<TFS.Common.WorkflowConfig.DeploymentScript>() { new TFS.Common.WorkflowConfig.DeploymentScript() { Role = "Desktop Client", Script = @"notepad.exe", WorkingDirectory = @"C:\temp" } } },
                TestDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.TestDetails() { TestPlanId = 3, TestSuiteIdList = new List<int>() { 3, 4 }, TestConfigurationId = 1 }
            });

            // Assert
            var buildDefinitions = new List<IBuildDefinition>(tfsbuild.QueryBuildDefinitions());
            Assert.IsTrue(buildDefinitions.Any(o => o.Name.Equals(name)));
        }
    }
}
