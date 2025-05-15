using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation for scheduled flow entities.
    /// Defines a complete executable flow unit.
    /// </summary>
    public abstract class AbstractScheduledFlowEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the scheduled flow identifier.
        /// </summary>
        public string ScheduledFlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the scheduled flow.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the scheduled flow.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow entity identifier.
        /// </summary>
        public string FlowEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source assignment entity identifier.
        /// </summary>
        public string SourceAssignmentEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination assignment entity identifier.
        /// </summary>
        public string DestinationAssignmentEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the task scheduler entity identifier.
        /// </summary>
        public string TaskSchedulerEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scheduled flow configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the scheduled flow metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the scheduled flow state.
        /// </summary>
        public ScheduledFlowState State { get; set; } = ScheduledFlowState.CREATED;

        /// <summary>
        /// Gets or sets the last execution time.
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the last execution status.
        /// </summary>
        public ExecutionStatus? LastExecutionStatus { get; set; }

        /// <summary>
        /// Gets or sets the execution count.
        /// </summary>
        public int ExecutionCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the scheduled flow is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public override string GetEntityId()
        {
            return ScheduledFlowId;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public override string GetEntityType()
        {
            return "ScheduledFlowEntity";
        }

        /// <summary>
        /// Validates the scheduled flow entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ScheduledFlowId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SCHEDULED_FLOW_ID_REQUIRED", Message = "Scheduled flow ID is required." });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SCHEDULED_FLOW_NAME_REQUIRED", Message = "Scheduled flow name is required." });
            }

            if (string.IsNullOrWhiteSpace(FlowEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FLOW_ENTITY_ID_REQUIRED", Message = "Flow entity ID is required." });
            }

            if (string.IsNullOrWhiteSpace(SourceAssignmentEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "SOURCE_ASSIGNMENT_ENTITY_ID_REQUIRED", Message = "Source assignment entity ID is required." });
            }

            if (string.IsNullOrWhiteSpace(DestinationAssignmentEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_ASSIGNMENT_ENTITY_ID_REQUIRED", Message = "Destination assignment entity ID is required." });
            }

            if (string.IsNullOrWhiteSpace(TaskSchedulerEntityId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "TASK_SCHEDULER_ENTITY_ID_REQUIRED", Message = "Task scheduler entity ID is required." });
            }

            // Validate entity relationships
            var relationshipValidation = ValidateEntityRelationships();
            if (!relationshipValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(relationshipValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates the relationships between the entities referenced by the scheduled flow.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateEntityRelationships();

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected override void CopyPropertiesTo(AbstractEntity target)
        {
            base.CopyPropertiesTo(target);

            if (target is AbstractScheduledFlowEntity scheduledFlowEntity)
            {
                scheduledFlowEntity.ScheduledFlowId = ScheduledFlowId;
                scheduledFlowEntity.Name = Name;
                scheduledFlowEntity.Description = Description;
                scheduledFlowEntity.FlowEntityId = FlowEntityId;
                scheduledFlowEntity.SourceAssignmentEntityId = SourceAssignmentEntityId;
                scheduledFlowEntity.DestinationAssignmentEntityId = DestinationAssignmentEntityId;
                scheduledFlowEntity.TaskSchedulerEntityId = TaskSchedulerEntityId;
                scheduledFlowEntity.Configuration = new Dictionary<string, object>(Configuration);
                scheduledFlowEntity.Metadata = new Dictionary<string, object>(Metadata);
                scheduledFlowEntity.State = State;
                scheduledFlowEntity.LastExecutionTime = LastExecutionTime;
                scheduledFlowEntity.LastExecutionStatus = LastExecutionStatus;
                scheduledFlowEntity.ExecutionCount = ExecutionCount;
                scheduledFlowEntity.IsEnabled = IsEnabled;
            }
        }
    }

    /// <summary>
    /// Represents the state of a scheduled flow.
    /// </summary>
    public enum ScheduledFlowState
    {
        /// <summary>
        /// Scheduled flow has been created but not yet initialized.
        /// </summary>
        CREATED,

        /// <summary>
        /// Scheduled flow is inactive.
        /// </summary>
        INACTIVE,

        /// <summary>
        /// Scheduled flow is active and running.
        /// </summary>
        ACTIVE,

        /// <summary>
        /// Scheduled flow is paused.
        /// </summary>
        PAUSED,

        /// <summary>
        /// Scheduled flow has failed.
        /// </summary>
        FAILED,

        /// <summary>
        /// Scheduled flow has been terminated.
        /// </summary>
        TERMINATED
    }

    /// <summary>
    /// Represents the status of a flow execution.
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// Execution has not started.
        /// </summary>
        NOT_STARTED,

        /// <summary>
        /// Execution is in progress.
        /// </summary>
        IN_PROGRESS,

        /// <summary>
        /// Execution has completed successfully.
        /// </summary>
        COMPLETED,

        /// <summary>
        /// Execution has failed.
        /// </summary>
        FAILED,

        /// <summary>
        /// Execution has been cancelled.
        /// </summary>
        CANCELLED,

        /// <summary>
        /// Execution has timed out.
        /// </summary>
        TIMED_OUT
    }
}
