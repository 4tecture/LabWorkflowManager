using System;
namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSLabEnvironment
    {
        void ChangeEnvironmentOwner(Uri labEnvironmentUri, string newOwner);
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedLabEnvironment> GetAssociatedLabEnvironments();
    }
}
