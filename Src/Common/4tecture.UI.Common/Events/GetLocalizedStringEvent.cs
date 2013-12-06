using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4tecture.UI.Common.Events
{
    public class GetLocalizedStringEvent : CompositePresentationEvent<LocalizedStringEventArgs>
    {
    }

    public class LocalizedStringEventArgs
    {
        private List<string> stringValues = new List<string>();

        public string Key { get; set; }
        public IEnumerable<string> Values { get { return this.stringValues; } }

        public void AddStringValue(string value)
        {
            this.stringValues.Add(value);
        }
    }
}
