using _4tecture.UI.Common.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.Services
{
    public class FileDialogService : IFileDialogService
    {
        public bool OpenFile(out string file)
        {
            file = string.Empty;
            var dlg = new OpenFileDialog();
            dlg.FileName = "MyLabWorkflowDefinition";
            dlg.DefaultExt = ".lwfdef";
            dlg.Filter ="Lab Workflow Definition (.lwfdef)|*.lwfdef";

            Nullable<bool> result = dlg.ShowDialog();
            if(result == true)
            {
                file = dlg.FileName;
                return true;
            }
            return false;
        }

        public bool SaveFile(out string file)
        {
            file = string.Empty;
            var dlg = new SaveFileDialog();
            dlg.FileName = "MyLabWorkflowDefinition";
            dlg.DefaultExt = ".lwfdef";
            dlg.Filter = "Lab Workflow Definition (.lwfdef)|*.lwfdef";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                file = dlg.FileName;
                return true;
            }
            return false;
        }
    }
}
