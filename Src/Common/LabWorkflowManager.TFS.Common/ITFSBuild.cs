using System;
using System.Collections.Generic;
namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSBuild
    {
        LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition CreateBuildDefinitionFromDefinition(LabWorkflowManager.TFS.Common.WorkflowConfig.LabWorkflowDefinitionDetails labworkflowDefinitionDetails);
        void DeleteBuildDefinition(params Uri[] uris);
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetMultiEnvAssociatedBuildDefinitions(Guid multiEnvConfigId);
        IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetAssociatedDefinitions();
        IEnumerable<string> GetProcessTemplateFiles();
        IEnumerable<string> GetBuildControllers();
    }
}
