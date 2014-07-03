using System.ComponentModel;
using LabWorkflowManager.TFS.Common.Resources;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4tecture.UI.Common.Converters;
using _4tecture.UI.Common.Helper;
using _4tecture.UI.Common.ViewModels;

namespace LabWorkflowManager.TFS.Common.WorkflowConfig
{
    public class LabBuildDefinitionDetails : NotificationObjectWithValidation
    {
        private string name;
        private string description;
        private string controllerName;
        private string processTemplateFilename;
        private BuildDefinitionContinuousIntegrationType continuousIntegrationType;
        private BuildDefinitionScheduledDays scheduledDays;
        private int quietPeriod;
        private int startTime;

        public string Name { get { return this.name; } set { this.name = value; VerifyStringNotEmpty(this.name, "Name", CommonStrings.ErrorLabBuildDefinitionDetailsName); this.RaisePropertyChanged(() => this.Name); } }

        public string Description { get { return this.description; } set { this.description = value; this.RaisePropertyChanged(() => this.Description); } }

        public string ControllerName { get { return this.controllerName; } set { this.controllerName = value; VerifyStringNotEmpty(this.controllerName, "ControllerName", CommonStrings.ErrorLabBuildDefinitionDetailsControllerName); this.RaisePropertyChanged(() => this.ControllerName); } }

        public string ProcessTemplateFilename { get { return this.processTemplateFilename; } set { this.processTemplateFilename = value; VerifyStringNotEmpty(this.processTemplateFilename, "ProcessTemplateFilename", CommonStrings.ErrorLabBuildDefinitionDetailsProcessTemplateFilename); this.RaisePropertyChanged(() => this.ProcessTemplateFilename); } }

        public BuildDefinitionContinuousIntegrationType ContinuousIntegrationType
        {
            get
            {
                return this.continuousIntegrationType;
            }
            set
            {
                this.continuousIntegrationType = value; 
                this.RaisePropertyChanged(() => this.ContinuousIntegrationType);
            }
        }

        public BuildDefinitionScheduledDays ScheduledDays { get { return this.scheduledDays; } set { this.scheduledDays = value; this.RaisePropertyChanged(() => this.ScheduledDays); } }

        public int QuietPeriod { get { return this.quietPeriod; } set { this.quietPeriod = value; this.RaisePropertyChanged(() => this.QuietPeriod); } }

        public int StartTime { get { return this.startTime; } set { this.startTime = value; this.RaisePropertyChanged(() => this.StartTime); } }

        internal LabBuildDefinitionDetails Clone()
        {
            var clone = new LabBuildDefinitionDetails();

            clone.Name = this.Name;
            clone.Description = this.Description;
            clone.ControllerName = this.ControllerName;
            clone.ProcessTemplateFilename = this.ProcessTemplateFilename;
            clone.ContinuousIntegrationType = this.ContinuousIntegrationType;
            clone.ScheduledDays = this.ScheduledDays;
            clone.QuietPeriod = this.QuietPeriod;
            clone.StartTime = this.StartTime;

            return clone;
        }


    }

    [Flags]
    public enum BuildDefinitionContinuousIntegrationType
    {
        // Summary:
        //     Continuous integeration not set.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_None", typeof(CommonStrings))]
        None = 1,
        //
        // Summary:
        //     Individual continuous integration.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_Individual", typeof(CommonStrings))]
        Individual = 2,
        //
        // Summary:
        //     Batch continuous integration.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_Batch", typeof(CommonStrings))]
        Batch = 4,
        //
        // Summary:
        //     Scheduled continuous integration only when changes occur.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_Schedule", typeof(CommonStrings))]
        Schedule = 8,
        //
        // Summary:
        //     Scheduled continuous integration even without changes.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_ScheduleForced", typeof(CommonStrings))]
        ScheduleForced = 16,
        //
        // Summary:
        //     Gated continuous integration.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_Gated", typeof(CommonStrings))]
        Gated = 32,
        //
        // Summary:
        //     All continuous integration types.
        [LocalizableDescription("BuildDefinitionContinuousIntegrationType_All", typeof(CommonStrings))]
        All = 63,
    }

    // Summary:
    //     Describes the schedule days.
    [Flags]
    public enum BuildDefinitionScheduledDays
    {
        // Summary:
        //     No scheduled days.
        None = 0,
        //
        // Summary:
        //     Monday.
        Monday = 1,
        //
        // Summary:
        //     Tuesday.
        Tuesday = 2,
        //
        // Summary:
        //     Wednesday.
        Wednesday = 4,
        //
        // Summary:
        //     Thursday.
        Thursday = 8,
        //
        // Summary:
        //     Friday.
        Friday = 16,
        //
        // Summary:
        //     Saturday.
        Saturday = 32,
        //
        // Summary:
        //     Sunday.
        Sunday = 64,
        //
        // Summary:
        //     Every day.
        All = 127,
    }
}
