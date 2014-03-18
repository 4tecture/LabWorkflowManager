using _4tecture.UI.Common.Events;
using LabWorkflowManager.TFS.Common;
using LabWorkflowManager.TFS.Common.WorkflowConfig;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LabWorkflowManager.Storage
{
    public class WorkflowManagerStorage : NotificationObject, IWorkflowManagerStorage
    {
        private IEventAggregator eventAggregator;

        public WorkflowManagerStorage(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        private object currentDefinitionFileLockObj = new object();
        private string currentDefinitionFile = null;
        private const string registryKeySettings = @"Software\4tecture\LabWorkflowManager\Settings";
        private const string recentFileRegistryName = @"RecentFile";

        private ObservableCollection<MultiEnvironmentWorkflowDefinition> definitions;
        public ObservableCollection<MultiEnvironmentWorkflowDefinition> Definitions
        {
            get { return this.definitions; }
            set { this.definitions = value; this.RaisePropertyChanged(() => this.Definitions); }
        }

        public string CurrentDefinitionFile
        {
            get
            {
                if (this.currentDefinitionFile == null)
                {
                    lock (this.currentDefinitionFileLockObj)
                    {
                        if (this.currentDefinitionFile == null)
                        {
                            Registry.CurrentUser.CreateSubKey(registryKeySettings);
                            RegistryKey settings = Registry.CurrentUser.OpenSubKey(registryKeySettings, false);
                            if (settings != null)
                            {
                                this.currentDefinitionFile = settings.GetValue(recentFileRegistryName, string.Empty).ToString();
                            }
                            this.RaisePropertyChanged(() => this.CurrentDefinitionFile);
                        }
                    }
                }
                return currentDefinitionFile;
            }
            private set
            {
                lock (this.currentDefinitionFileLockObj)
                {
                    RegistryKey settings = Registry.CurrentUser.CreateSubKey(registryKeySettings);
                    if (settings != null)
                    {
                        settings.SetValue(recentFileRegistryName, value, RegistryValueKind.String);
                    }
                    this.currentDefinitionFile = value;
                }
            }
        }

        private object lastTFSConnectionLockObj = new object();
        private TFSConnectionDetails lastTFSConnection = null;
        private const string recentTfsConnectionRegistryName = @"RecentTfsConnection";
        public TFSConnectionDetails LastTFSConnection
        {
            get
            {
                if (this.lastTFSConnection == null)
                {
                    lock (this.lastTFSConnectionLockObj)
                    {
                        if (this.lastTFSConnection == null)
                        {
                            RegistryKey settings = Registry.CurrentUser.OpenSubKey(registryKeySettings, false);
                            if (settings != null)
                            {
                                var tfsconn = settings.GetValue(recentTfsConnectionRegistryName, string.Empty).ToString();
                                if (!string.IsNullOrWhiteSpace(tfsconn))
                                {
                                    this.lastTFSConnection = new TFSConnectionDetails() { Uri = tfsconn.Substring(0, tfsconn.LastIndexOf("/")), Project = tfsconn.Substring(tfsconn.LastIndexOf("/") + 1) };
                                }
                            }
                            this.RaisePropertyChanged(() => this.LastTFSConnection);
                        }
                    }
                }
                return this.lastTFSConnection;
            }
            set
            {
                lock (this.lastTFSConnectionLockObj)
                {
                    RegistryKey settings = Registry.CurrentUser.CreateSubKey(registryKeySettings);
                    if (settings != null)
                    {
                        settings.SetValue(recentTfsConnectionRegistryName, string.Format("{0}/{1}", value.Uri, value.Project), RegistryValueKind.String);
                    }
                    this.lastTFSConnection = value;
                }
            }
        }

        public void Load(string pathToFile)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MultiEnvironmentWorkflowDefinition>));
                using (TextReader textReader = new StreamReader(pathToFile))
                {
                    this.Definitions = serializer.Deserialize(textReader) as ObservableCollection<MultiEnvironmentWorkflowDefinition>;
                    textReader.Close();
                }
                this.CurrentDefinitionFile = pathToFile;
            }
            catch (Exception)
            {
                this.Definitions = null;
                this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotLoadDefinitions");
            }
        }


        public void New(string pathToFile)
        {
            this.Definitions = new ObservableCollection<MultiEnvironmentWorkflowDefinition>();
            this.Save(pathToFile);
        }

        public void Save(string pathToFile)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MultiEnvironmentWorkflowDefinition>));
                using (TextWriter textWriter = new StreamWriter(pathToFile))
                {
                    serializer.Serialize(textWriter, this.Definitions);
                    textWriter.Close();
                }
                this.CurrentDefinitionFile = pathToFile;
            }
            catch (Exception)
            {
                this.eventAggregator.GetEvent<ShowMessageEvent>().Publish("#MsgCannotSaveDefinitions");
            }
        }


    }
}
