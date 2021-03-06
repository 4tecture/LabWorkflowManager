﻿using _4tecture.UI.Common.DependencyInjection;
using LabWorkflowManager.TFS.Common;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS2013.Modularity
{
    public class TFS2013Module : IModule
    {
        private IDependencyContainer container;

        public TFS2013Module(IDependencyContainer container, IRegionManager regionManager)
        {
            this.container = container;
        }

        public void Initialize()
        {
            this.container.RegisterInstance<ITFSConnectivity>(this.container.TryResolve<TFSConnectivity>());
            this.container.RegisterType<ITFSBuild, TFSBuild>();
            this.container.RegisterType<ITFSLabEnvironment, TFSLabEnvironment>();
            this.container.RegisterType<ITFSTest, TFSTest>();
        }
    }
}
