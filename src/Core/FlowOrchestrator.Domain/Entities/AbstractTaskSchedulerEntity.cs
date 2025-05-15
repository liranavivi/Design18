using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for task scheduler entities.
    /// Defines an active scheduling component.
    /// </summary>
    public abstract class AbstractTaskSchedulerEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the task scheduler identifier.
        /// </summary>
        public string SchedulerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the task scheduler.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the task scheduler.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schedule type.
        /// </summary>
        public ScheduleType ScheduleType { get; set; } = ScheduleType.ONCE;

        /// <summary>
        /// Gets or sets the schedule expression.
        /// </summary>
        public string ScheduleExpression { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start date and time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        public string TimeZone { get; set; } = "UTC";

        /// <summary>
        /// Gets or sets the task scheduler configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the task scheduler metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the task scheduler state.
        /// </summary>
        public SchedulerState State { get; set; } = SchedulerState.CREATED;

        /// <summary>
        /// Gets or sets the last execution time.
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the next execution time.
        /// </summary>
        public DateTime? NextExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the execution count.
        /// </summary>
        public int ExecutionCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the task scheduler is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return SchedulerId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "TaskSchedulerEntity";
        }

        /// <summary>
        /// Validates the task scheduler entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(SchedulerId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SCHEDULER_ID_REQUIRED", Message = "Task scheduler ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SCHEDULER_NAME_REQUIRED", Message = "Task scheduler name is required." });
            }

            if (string.IsNullOrWhiteSpace(ScheduleExpression))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SCHEDULE_EXPRESSION_REQUIRED", Message = "Schedule expression is required." });
            }

            // Validate schedule expression based on schedule type
            var scheduleExpressionValidation = ValidateScheduleExpression();
            if (!scheduleExpressionValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(scheduleExpressionValidation.Errors);
            }

            // Validate date range
            if (StartDateTime.HasValue && EndDateTime.HasValue && StartDateTime > EndDateTime)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_DATE_RANGE", Message = "Start date/time must be before end date/time." });
            }

            return result;
        }

        /// <summary>
        /// Validates the schedule expression based on the schedule type.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateScheduleExpression();

        /// <summary>
        /// Calculates the next execution time based on the schedule expression.
        /// </summary>
        /// <returns>The next execution time.</returns>
        public abstract DateTime? CalculateNextExecutionTime();

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractTaskSchedulerEntity schedulerEntity)
            {
                schedulerEntity.SchedulerId = SchedulerId;
                schedulerEntity.Name = Name;
                schedulerEntity.Description = Description;
                schedulerEntity.ScheduleType = ScheduleType;
                schedulerEntity.ScheduleExpression = ScheduleExpression;
                schedulerEntity.StartDateTime = StartDateTime;
                schedulerEntity.EndDateTime = EndDateTime;
                schedulerEntity.TimeZone = TimeZone;
                schedulerEntity.Configuration = new Dictionary<string, object>(Configuration);
                schedulerEntity.Metadata = new Dictionary<string, object>(Metadata);
                schedulerEntity.State = State;
                schedulerEntity.LastExecutionTime = LastExecutionTime;
                schedulerEntity.NextExecutionTime = NextExecutionTime;
                schedulerEntity.ExecutionCount = ExecutionCount;
                schedulerEntity.IsEnabled = IsEnabled;
            }
        }
    }

    /// <summary>
    /// Represents the type of schedule.
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// One-time schedule.
        /// </summary>
        ONCE,

        /// <summary>
        /// Recurring schedule using cron expression.
        /// </summary>
        CRON,

        /// <summary>
        /// Recurring schedule with fixed interval.
        /// </summary>
        INTERVAL,

        /// <summary>
        /// Custom schedule type.
        /// </summary>
        CUSTOM
    }

    /// <summary>
    /// Represents the state of a task scheduler.
    /// </summary>
    public enum SchedulerState
    {
        /// <summary>
        /// Scheduler has been created but not yet initialized.
        /// </summary>
        CREATED,

        /// <summary>
        /// Scheduler is inactive.
        /// </summary>
        INACTIVE,

        /// <summary>
        /// Scheduler is active and running.
        /// </summary>
        ACTIVE,

        /// <summary>
        /// Scheduler is paused.
        /// </summary>
        PAUSED,

        /// <summary>
        /// Scheduler has failed.
        /// </summary>
        FAILED,

        /// <summary>
        /// Scheduler has been terminated.
        /// </summary>
        TERMINATED
    }
}
