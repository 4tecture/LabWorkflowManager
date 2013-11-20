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
            var tfsbuild = new TFSBuild();
            tfsbuild.Connectivity.Connect("http://vsalm:8080/tfs/FabrikamFiberCollection", "FabrikamFiber");

            var name = string.Format("Test BDT Integration {0}", DateTime.Now.Ticks);
            var description = string.Format("Description for Build Definition created by Integration Test {0}", DateTime.Now.Ticks);

            // Act
            tfsbuild.CreateBuildDefinition(name,
                description,
                "VSALM",
                "LabDefaultTemplate.11.xaml",
                new TFS.Common.SourceBuildDetails() { BuildDefinitionUri = new Uri("vstfs:///Build/Definition/1") },
                new TFS.Common.EnvironmentDetails() { LabEnvironmentUri = new Uri("vstfs:///LabManagement/LabEnvironment/2") },
                new TFS.Common.DeploymentDetails() { Scripts = new System.Collections.Generic.List<TFS.Common.DeploymentScript>() { new TFS.Common.DeploymentScript() { Role = "Desktop Client", Script = @"notepad.exe", WorkingDirectory = @"C:\temp" } } },
                new TFS.Common.TestDetails()
                );

            // Assert
            var buildDefinitions = new List<IBuildDefinition>(tfsbuild.QueryBuildDefinitions());
            Assert.IsTrue(buildDefinitions.Any(o => o.Name.Equals(name)));
        }
    }
}
