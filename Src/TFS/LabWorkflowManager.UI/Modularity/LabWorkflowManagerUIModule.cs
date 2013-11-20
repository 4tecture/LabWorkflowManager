using _4tecture.UI.Common.DependencyInjection;
using _4tecture.UI.Common.Helper;
using LabWorkflowManager.UI.ViewModels;
using LabWorkflowManager.UI.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.UI.Modularity
{


    public class LabWorkflowManagerUIModule : IModule
    {
        private IDependencyContainer container;

        public LabWorkflowManagerUIModule(IDependencyContainer container, IRegionManager regionManager)
        {
            this.container = container;
        }

        public void Initialize()
        {
            DataTemplateHelper.RegisterAndCreateTemplate<MultiEnvironmentWorkflowsViewModel, MultiEnvironmentWorkflowsView>(this.container, RegionNames.MainRegion);
            DataTemplateHelper.RegisterAndCreateTemplate<MultiEnvironmentWorkflowDefinitionViewModel, MultiEnvironmentWorkflowDefinitionView>(this.container);
        }
    }
}
