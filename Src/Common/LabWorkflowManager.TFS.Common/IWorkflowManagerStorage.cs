using System;
using System.ComponentModel;
namespace LabWorkflowManager.TFS.Common
{
    public interface IWorkflowManagerStorage : INotifyPropertyChanged
    {
        System.Collections.ObjectModel.ObservableCollection<LabWorkflowManager.TFS.Common.WorkflowConfig.MultiEnvironmentWorkflowDefinition> Definitions { get; set; }
        string CurrentDefinitionFile { get; }

        void New(string pathToFile);
        void Load(string pathToFile);
        void Save(string pathToFile);

        TFSConnectionDetails LastTFSConnection { get; set; }
    }

    public class TFSConnectionDetails
    {
        public string Uri { get; set; }
        public string Project { get; set; }
    }
}
