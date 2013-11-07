using _4tecture.UI.Common.Events;
using LabWorkflowManager.Resources;
using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LabWorkflowManager.Handlers
{
    public class MsgBoxHandler
    {
        public MsgBoxHandler(IEventAggregator eventaggregator)
        {
            eventaggregator.GetEvent<ShowMessageEvent>().Subscribe(msg => MessageBox.Show(msg, ApplicationStrings.MsgBoxTitle));
        }
    }
}
