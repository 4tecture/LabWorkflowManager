using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.Config
{
    public class LabWorkflowManagerSettings : CustomSettingsBase, ILabWorkflowManagerSettings
    {
        private static object lockObject = new object();
        private static LabWorkflowManagerSettings settings;
        public static LabWorkflowManagerSettings CurrentSettings
        {
            get
            {
                if (settings == null)
                {
                    lock (lockObject)
                    {
                        if (settings == null)
                        {
                            settings = GetCurrentSettings<LabWorkflowManagerSettings>("LabWorkflowManagerSettings");
                        }
                    }
                }
                return settings;
            }
        }
    }
}
