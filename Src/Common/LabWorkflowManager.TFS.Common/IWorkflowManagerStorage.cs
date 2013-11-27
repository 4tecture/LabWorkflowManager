using System;
namespace LabWorkflowManager.TFS.Common
{
    public interface IWorkflowManagerStorage
    {
        System.Collections.ObjectModel.ObservableCollection<LabWorkflowManager.TFS.Common.WorkflowConfig.MultiEnvironmentWorkflowDefinition> Definitions { get; set; }
        void Load();
        void Save();
    }
}
