using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.Config
{
    public class CustomSettingsBase : ConfigurationSection
    {
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

        protected ILoggerFacade Logger { get; set; }

        public CustomSettingsBase()
        {
            this.Logger = SetupLogger();
        }

        public void Save()
        {
            if (config != null)
            {
                config.Save();
            }
        }

        protected virtual ILoggerFacade SetupLogger()
        {
            ILoggerFacade logInstance = new NullLogger();
            try
            {
                var tmplogInstance = ServiceLocator.Current.GetInstance<ILoggerFacade>();
                if (tmplogInstance != null)
                {
                    logInstance = tmplogInstance;
                }
            }
            catch (Exception)
            {
            }
            return logInstance;
        }

        protected static T GetCurrentSettings<T>(string sectionName) where T : CustomSettingsBase
        {
            return config.GetSection(sectionName) as T;
        }

        private class NullLogger : ILoggerFacade
        {
            public void Log(string message, Category category, Priority priority)
            {

            }
        }

    }
}
