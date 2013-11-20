using _4tecture.UI.Common.DependencyInjection;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2012.Modularity
{
    public class TFS2012Module : IModule
    {
        private IDependencyContainer container;

        public TFS2012Module(IDependencyContainer container, IRegionManager regionManager)
        {
            this.container = container;
        }

        public void Initialize()
        {
            //DataTemplateHelper.RegisterAndCreateTemplate<WarehouseControlViewModel, WarehouseControlView>(this.container);
            //DataTemplateHelper.RegisterAndCreateTemplate<MainReportingViewModel, MainReportingView>(this.container,RegionNames.MainRegion);
        }
    }
}
