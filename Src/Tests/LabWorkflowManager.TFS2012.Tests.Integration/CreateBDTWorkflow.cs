using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using System.Linq;
using System.Collections.ObjectModel;
using Moq;
using Microsoft.Practices.Prism.Events;
using LabWorkflowManager.Storage;

namespace LabWorkflowManager.TFS2012.Tests.Integration
{
    [TestClass]
    public class CreateBDTWorkflow
    {
        [TestMethod]
        public void CreateBDTWorkflowFromScratch()
        {
            // Arrange
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var tfsbuild = new TFSBuild(new TFSConnectivity(new WorkflowManagerStorage(eventAggregatorMock.Object), eventAggregatorMock.Object));
            tfsbuild.Connectivity.Connect("http://vsalm:8080/tfs/FabrikamFiberCollection", "FabrikamFiber");

            var name = string.Format("Test BDT Integration {0}", DateTime.Now.Ticks);
            var description = string.Format("Description for Build Definition created by Integration Test {0}", DateTime.Now.Ticks);

            // Act
            tfsbuild.CreateBuildDefinition(new LabWorkflowManager.TFS.Common.WorkflowConfig.LabWorkflowDefinitionDetails(){
                LabBuildDefinitionDetails =  new LabWorkflowManager.TFS.Common.WorkflowConfig.LabBuildDefinitionDetails() { Name = name, Description = description, ControllerName = "VSALM", ProcessTemplateFilename = "LabDefaultTemplate.11.xaml", ContinuousIntegrationType = TFS.Common.WorkflowConfig.BuildDefinitionContinuousIntegrationType.None },
                SourceBuildDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.SourceBuildDetails() { BuildDefinitionUri = "vstfs:///Build/Definition/1" },
                LabEnvironmentDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.LabEnvironmentDetails() { LabEnvironmentUri = "vstfs:///LabManagement/LabEnvironment/2" },
                DeploymentDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.DeploymentDetails() { Scripts = new ObservableCollection<TFS.Common.WorkflowConfig.DeploymentScript>() { new TFS.Common.WorkflowConfig.DeploymentScript() { Role = "Desktop Client", Script = @"notepad.exe", WorkingDirectory = @"C:\temp" } } },
                TestDetails = new LabWorkflowManager.TFS.Common.WorkflowConfig.TestDetails() { TestPlanId = 3, TestSuiteIdList = new ObservableCollection<int>() { 3, 4 }, TestConfigurationId = 1 }
            });

            // Assert
            var buildDefinitions = new List<IBuildDefinition>(tfsbuild.QueryBuildDefinitions());
            Assert.IsTrue(buildDefinitions.Any(o => o.Name.Equals(name)));
        }
    }
}
