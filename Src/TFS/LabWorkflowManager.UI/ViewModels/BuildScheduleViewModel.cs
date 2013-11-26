using LabWorkflowManager.TFS.Common.WorkflowConfig;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabWorkflowManager.UI.ViewModels
{
    public class BuildScheduleViewModel : NotificationObject
    {
        public BuildScheduleViewModel(TFS.Common.WorkflowConfig.MultiEnvironmentWorkflowDefinition multiEnvironmentWorkflowDefinition)
        {
            // TODO: Complete member initialization
            this.Item = multiEnvironmentWorkflowDefinition;

            this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.PropertyChanged += (sender, args) => { if (args.PropertyName.Equals("ContinuousIntegrationType")) { this.RaisePropertyChanged(() => this.QuietPeriodVisible); this.RaisePropertyChanged(() => this.ScheduledDaysVisible); } };
        }
        public bool ScheduledDayMonday
        {

            get { return IsDaySelected(BuildDefinitionScheduledDays.Monday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Monday); }
        }

        public bool ScheduledDayTuesday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Tuesday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Tuesday); }
        }

        public bool ScheduledDayWednesday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Wednesday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Wednesday); }
        }

        public bool ScheduledDayThursday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Thursday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Thursday); }
        }

        public bool ScheduledDayFriday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Friday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Friday); }
        }

        public bool ScheduledDaySaturday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Saturday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Saturday); }
        }

        public bool ScheduledDaySunday
        {
            get { return IsDaySelected(BuildDefinitionScheduledDays.Sunday); }
            set { SetSelectedDay(value, BuildDefinitionScheduledDays.Sunday); }
        }

        private void SetSelectedDay(bool value, BuildDefinitionScheduledDays selectedDay)
        {
            if (value)
            {
                this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ScheduledDays = this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ScheduledDays | selectedDay;
            }
            else
            {
                this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ScheduledDays &= ~selectedDay;
            }
        }

        private bool IsDaySelected(BuildDefinitionScheduledDays selectedDay)
        {
            return this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ScheduledDays.HasFlag(selectedDay) || this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ScheduledDays.HasFlag(BuildDefinitionScheduledDays.All);
        }

        public bool QuietPeriodVisible
        {
            get
            {
                return this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ContinuousIntegrationType == BuildDefinitionContinuousIntegrationType.Batch;
            }
        }

        public bool ScheduledDaysVisible
        {
            get
            {
                return this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ContinuousIntegrationType == BuildDefinitionContinuousIntegrationType.Schedule || this.Item.MainLabWorkflowDefinition.LabBuildDefinitionDetails.ContinuousIntegrationType == BuildDefinitionContinuousIntegrationType.ScheduleForced;
            }
        }

        public TFS.Common.WorkflowConfig.MultiEnvironmentWorkflowDefinition Item { get; set; }
    }
}
