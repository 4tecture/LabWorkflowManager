using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class AssociatedLabEnvironment
    {
        public string Uri { get; set; }
        public List<string> Snapshots { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssociatedLabEnvironment;
            if (other == null)
            {
                return false;
            }
            return this.Uri.Equals(other.Uri);
        }

        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        public List<string> Roles { get; set; }
    }
}
