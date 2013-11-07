using LabWorkflowManager.Bootstrapping;
using LabWorkflowManager.Resources;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LabWorkflowManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IConfigurationSource config = ConfigurationSourceFactory.Create();
            Logger.SetLogWriter(new LogWriterFactory(config).Create());
            ExceptionPolicyFactory factory = new ExceptionPolicyFactory(config);
            ExceptionPolicy.SetExceptionManager(factory.CreateManager());

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomainUnhandledException);
            try
            {
                Bootstrapper applicationBootstrapper = new Bootstrapper();
                applicationBootstrapper.Run();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void HandleException(Exception ex)
        {
            if (ex == null)
                return;

            ExceptionPolicy.HandleException(ex, "Default Policy");
            MessageBox.Show(ApplicationStrings.UnhandledException + ex.StackTrace, ApplicationStrings.MsgBoxTitle);
            Environment.Exit(1);
        }
    }
}
