using LabWorkflowManager.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.ViewModels
{
    public class ShellViewModel
    {
        public string ApplicationInformation
        {
            get
            {
                string version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
                return string.Format(ApplicationStrings.ApplicationInformation, version);
            }
        }
    }
}
