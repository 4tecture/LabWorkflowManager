using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabWorkflowManager.TFS.Common.WorkflowConfig;

namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSBuild
    {
        Task<AssociatedBuildDefinition> CreateBuildDefinitionFromDefinition(LabWorkflowDefinitionDetails labworkflowDefinitionDetails);
        void DeleteBuildDefinition(params Uri[] uris);
        System.Collections.Generic.IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetMultiEnvAssociatedBuildDefinitions(Guid multiEnvConfigId);
        IEnumerable<LabWorkflowManager.TFS.Common.WorkflowConfig.AssociatedBuildDefinition> GetAssociatedDefinitions();
        IEnumerable<string> GetProcessTemplateFiles();
        IEnumerable<string> GetBuildControllers();
    }
}
