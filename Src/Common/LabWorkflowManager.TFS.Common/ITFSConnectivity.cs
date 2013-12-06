using System;
using System.ComponentModel;
namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSConnectivity : INotifyPropertyChanged
    {
        void ConnectUI();
        void Connect(string tfsUri, string projectName);
        bool IsConnected { get; }

        bool IsConnecting { get; }

        string TfsUri { get; }
        string TeamProjectName { get; }
    }
}
