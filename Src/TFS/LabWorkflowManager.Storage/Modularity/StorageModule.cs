using _4tecture.UI.Common.DependencyInjection;
using LabWorkflowManager.Storage;
using LabWorkflowManager.TFS.Common;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.Storage.Modularity
{
    public class StorageModule : IModule
    {
        private IDependencyContainer container;

        public StorageModule(IDependencyContainer container, IRegionManager regionManager)
        {
            this.container = container;
        }

        public void Initialize()
        {
            this.container.RegisterInstance<IWorkflowManagerStorage>(this.container.TryResolve<WorkflowManagerStorage>());
        }
    }
}
