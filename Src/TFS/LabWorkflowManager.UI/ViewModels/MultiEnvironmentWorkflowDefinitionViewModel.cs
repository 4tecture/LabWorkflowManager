using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.UI.ViewModels
{
    public class MultiEnvironmentWorkflowDefinitionViewModel : NotificationObject
    {
        private ITFSConnectivity tfsConnectivity;
        private ITFSBuild tfsBuild;
        private ITFSLabEnvironment tfsLabEnvironment;
        private ITFSTest tfsTest;
        private IWorkflowManagerStorage workflowManagerStorage;
        private IRegionManager regionManager;
        public MultiEnvironmentWorkflowDefinitionViewModel(MultiEnvironmentWorkflowDefinition item, ITFSConnectivity tfsConnectivity, ITFSBuild tfsBuild, ITFSLabEnvironment tfsLabEnvironment, ITFSTest tfsTest, IRegionManager regionManager)
        {
            this.Item = item;
            this.Item.PropertyChanged += (sender, args) => {if(args.PropertyName.Equals("Name")) this.RaisePropertyChanged(()=>this.HeaderInfo);};
        }

        public MultiEnvironmentWorkflowDefinition Item { get; set; }

        public string HeaderInfo
        {
            get
            {
                return ModuleStrings.TitleEdit + " " + Item.Name;
            }
        }

    }
}
