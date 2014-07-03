using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;

namespace LabWorkflowManager.TFS.Common.GlobalCommands
{
    public static class CompositeApplicationCommands
    {
        public static readonly CompositeCommand SaveAllCommand = new CompositeCommand();
        public static readonly CompositeCommand RefreshCommand = new CompositeCommand();
    }
}
