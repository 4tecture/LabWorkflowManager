﻿using _4tecture.UI.Common.DependencyInjection;
using _4tecture.UI.Common.Helper;
using LabWorkflowManager.Handlers;
using LabWorkflowManager.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LabWorkflowManager.Bootstrapping
{
    class Bootstrapper : UnityBootstrapper
    {

        protected override Microsoft.Practices.Prism.Modularity.IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override Microsoft.Practices.Prism.Logging.ILoggerFacade CreateLogger()
        {
            return new EnterpriseLibraryLoggerAdapter();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        protected override void InitializeModules()
        {
            this.Container.RegisterType<IDependencyContainer, DependencyContainerAdapeter>();
            this.Container.RegisterInstance<LabWorkflowManager.Config.ILabWorkflowManagerSettings>(LabWorkflowManager.Config.LabWorkflowManagerSettings.CurrentSettings);
            
            //DataTemplateHelper.RegisterAndCreateTemplate<LabWorkflowManager.ViewModels.ConfigurationViewModel, LabWorkflowManager.Views.ConfigurationView>(this.Container.Resolve<IDependencyContainer>(), RegionNames.MainRegion);

            var msgBoxHandler = this.Container.Resolve<MsgBoxHandler>();
            this.Container.RegisterInstance(msgBoxHandler);

            base.InitializeModules();

        }

        protected override System.Windows.DependencyObject CreateShell()
        {
            var view = this.Container.TryResolve<Shell>();
            Application.Current.MainWindow = view;
            view.Show();
            return view;
        }
    }
}