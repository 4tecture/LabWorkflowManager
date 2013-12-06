using _4tecture.UI.Common.Events;
using LabWorkflowManager.UI.Resources;
using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.UI.Modularity
{
    public class ModuleStringsHandler
    {
        public ModuleStringsHandler(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<GetLocalizedStringEvent>().Subscribe(GetLocalizedString);
        }

        private void GetLocalizedString(LocalizedStringEventArgs args)
        {
            var stringValue = ModuleStrings.ResourceManager.GetString(args.Key);
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                args.AddStringValue(stringValue);
            }
        }
    }
}
