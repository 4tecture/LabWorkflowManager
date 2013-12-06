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
        private IEventAggregator eventaggregator;
        public MsgBoxHandler(IEventAggregator eventaggregator)
        {
            this.eventaggregator = eventaggregator;
            this.eventaggregator.GetEvent<ShowMessageEvent>().Subscribe(ShowMessage);
        }

        private void ShowMessage(string msg)
        {
            if (msg != null && msg.StartsWith("#"))
            {
                var args = new LocalizedStringEventArgs() { Key = msg.Substring(1) };
                eventaggregator.GetEvent<GetLocalizedStringEvent>().Publish(args);
                msg = args.Values.FirstOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(msg))
            {
                MessageBox.Show(msg, ApplicationStrings.MsgBoxTitle);
            }
        }
    }
}
