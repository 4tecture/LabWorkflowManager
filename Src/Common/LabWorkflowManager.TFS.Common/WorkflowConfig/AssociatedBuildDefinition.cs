using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class AssociatedBuildDefinition
    {
		public AssociatedBuildDefinition()
		{
			this.Builds = new List<AssociatedBuildDetail>();
		}
        public String BuildControllerName { get; set; }

        public string BuildControllerUri { get; set; }

        public int ContinuousIntegrationQuietPeriod { get; set; }

        public BuildDefinitionContinuousIntegrationType ContinuousIntegrationType { get; set; }

        public DateTime DateCreated { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }

        public string LastBuildUri { get; set; }

        public string LastGoodBuildLabel { get; set; }

        public string LastGoodBuildUri { get; set; }

        public string Uri { get; set; }

        public List<AssociatedBuildDetail> Builds { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssociatedBuildDefinition;
            if(other == null)
            {
                return false;
            }
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        
    }

    public class AssociatedBuildDetail
    {

        public string LabelName { get; set; }

        public string Uri { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssociatedBuildDetail;
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
    }
}
