using System;
namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSTest
    {
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestConfiguration> GetAssociatedTestConfigurations();
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestPlan> GetAssociatedTestPlans();
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSettings> GetAssociatedTestSettings();
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedTestSuite> GetAssociatedTestSuites(int testplanId);
    }
}
