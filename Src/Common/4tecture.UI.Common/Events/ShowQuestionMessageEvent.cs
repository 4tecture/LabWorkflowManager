using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4tecture.UI.Common.Events
{
    public class ShowQuestionMessageEvent : CompositePresentationEvent<ShowQuestionMessageArgs>
    {
    }

    public class ShowQuestionMessageArgs
    {
        public string Msg{ get; set; }
        public MessageResult Result { get; set; }
    }

    public enum MessageResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }
}
