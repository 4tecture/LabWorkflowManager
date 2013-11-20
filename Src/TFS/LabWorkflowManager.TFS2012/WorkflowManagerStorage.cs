using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LabWorkflowManager.TFS2012
{
    public class WorkflowManagerStorage : IWorkflowManagerStorage
    {
        public ObservableCollection<MultiEnvironmentWorkflowDefinition> Definitions { get; set; }

        public void Load()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MultiEnvironmentWorkflowDefinition>));
                using (TextReader textReader = new StreamReader(@"C:\Temp\MultiEnvironmentWorklowDefinition.xml"))
                {
                    this.Definitions = serializer.Deserialize(textReader) as ObservableCollection<MultiEnvironmentWorkflowDefinition>;
                    textReader.Close();
                }
            }
            catch(Exception ex)
            {
                this.Definitions = new ObservableCollection<MultiEnvironmentWorkflowDefinition>();
            }
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MultiEnvironmentWorkflowDefinition>));
            using (TextWriter textWriter = new StreamWriter(@"C:\Temp\MultiEnvironmentWorklowDefinition.xml"))
            {
                serializer.Serialize(textWriter, this.Definitions);
                textWriter.Close();
            }
        }
    }
}
