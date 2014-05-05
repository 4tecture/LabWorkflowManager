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
            this.eventaggregator.GetEvent<ShowQuestionMessageEvent>().Subscribe(ShowQuestionMessage);
        }

        private void ShowMessage(string msg)
        {
            msg = GetMsgText(msg);

            if (!string.IsNullOrWhiteSpace(msg))
            {
                MessageBox.Show(msg, ApplicationStrings.MsgBoxTitle);
            }
        }

        private string GetMsgText(string msg)
        {
            if (msg != null && msg.StartsWith("#"))
            {
                var args = new LocalizedStringEventArgs() {Key = msg.Substring(1)};
                eventaggregator.GetEvent<GetLocalizedStringEvent>().Publish(args);
                msg = args.Values.FirstOrDefault();
            }
            return msg;
        }

        private void ShowQuestionMessage(ShowQuestionMessageArgs args)
        {
            var msg = GetMsgText(args.Msg);            
            var result = MessageBox.Show(msg, ApplicationStrings.MsgBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            args.Result = (MessageResult)result;
        }
    }
}
