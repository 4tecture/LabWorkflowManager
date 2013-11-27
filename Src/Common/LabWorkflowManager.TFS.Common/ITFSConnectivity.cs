using System;
namespace LabWorkflowManager.TFS.Common
{
    public interface ITFSConnectivity
    {
        void ConnectUI();
        void Connect(string tfsUri, string projectName);
        bool IsConnected { get; }

        string TfsUri { get; }
        string TeamProjectName { get; }
    }
}
