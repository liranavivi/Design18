using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractScheduledFlowEntity class.
    /// Defines a complete executable flow unit.
    /// </summary>
    public class ScheduledFlowEntity : AbstractScheduledFlowEntity
    {
        /// <summary>
        /// Gets or sets the execution priority.
        /// </summary>
        public int ExecutionPriority { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 3600;

        /// <summary>
        /// Gets or sets a value indicating whether to retry on failure.
        /// </summary>
        public bool RetryOnFailure { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry delay in seconds.
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets the notification settings.
        /// </summary>
        public ScheduledFlowNotificationSettings Notifications { get; set; } = new ScheduledFlowNotificationSettings();

        /// <summary>
        /// Gets or sets the execution history retention days.
        /// </summary>
        public int ExecutionHistoryRetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the execution parameters.
        /// </summary>
        public Dictionary<string, object> ExecutionParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution statistics.
        /// </summary>
        public ExecutionStatistics Statistics { get; set; } = new ExecutionStatistics();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntity"/> class.
        /// </summary>
        public ScheduledFlowEntity()
        {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntity"/> class with the specified scheduled flow ID.
        /// </summary>
        /// <param name="scheduledFlowId">The scheduled flow identifier.</param>
        public ScheduledFlowEntity(string scheduledFlowId)
        {
            ScheduledFlowId = scheduledFlowId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntity"/> class with the specified scheduled flow ID and name.
        /// </summary>
        /// <param name="scheduledFlowId">The scheduled flow identifier.</param>
        /// <param name="name">The name of the scheduled flow.</param>
        public ScheduledFlowEntity(string scheduledFlowId, string name) : this(scheduledFlowId)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntity"/> class with the specified scheduled flow ID, name, and related entity IDs.
        /// </summary>
        /// <param name="scheduledFlowId">The scheduled flow identifier.</param>
        /// <param name="name">The name of the scheduled flow.</param>
        /// <param name="flowEntityId">The flow entity identifier.</param>
        /// <param name="sourceAssignmentEntityId">The source assignment entity identifier.</param>
        /// <param name="destinationAssignmentEntityId">The destination assignment entity identifier.</param>
        /// <param name="taskSchedulerEntityId">The task scheduler entity identifier.</param>
        public ScheduledFlowEntity(string scheduledFlowId, string name, string flowEntityId, string sourceAssignmentEntityId, string destinationAssignmentEntityId, string taskSchedulerEntityId) : this(scheduledFlowId, name)
        {
            FlowEntityId = flowEntityId;
            SourceAssignmentEntityId = sourceAssignmentEntityId;
            DestinationAssignmentEntityId = destinationAssignmentEntityId;
            TaskSchedulerEntityId = taskSchedulerEntityId;
        }

        /// <summary>
        /// Validates the relationships between the entities referenced by the scheduled flow.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateEntityRelationships()
        {
            var result = new ValidationResult { IsValid = true };

            // In a real implementation, this would validate that:
            // 1. The flow entity exists and is valid
            // 2. The source assignment entity exists and is valid
            // 3. The destination assignment entity exists and is valid
            // 4. The task scheduler entity exists and is valid
            // 5. The source assignment is compatible with the flow's importer
            // 6. The destination assignment is compatible with the flow's exporter
            // 7. The flow, source assignment, and destination assignment are all enabled

            // For this example, we'll just perform some basic validation

            // Validate execution priority
            if (ExecutionPriority < 1 || ExecutionPriority > 10)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_EXECUTION_PRIORITY", Message = "Execution priority must be between 1 and 10." });
            }

            // Validate timeout
            if (TimeoutSeconds <= 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_TIMEOUT", Message = "Timeout must be greater than zero seconds." });
            }

            // Validate retry settings
            if (RetryOnFailure)
            {
                if (MaxRetries <= 0)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_MAX_RETRIES", Message = "Maximum retries must be greater than zero when retry on failure is enabled." });
                }

                if (RetryDelaySeconds <= 0)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "INVALID_RETRY_DELAY", Message = "Retry delay must be greater than zero seconds when retry on failure is enabled." });
                }
            }

            // Validate notification settings
            if ((Notifications.NotifyOnSuccess || Notifications.NotifyOnFailure) &&
                Notifications.NotificationEmails.Count == 0 &&
                string.IsNullOrWhiteSpace(Notifications.WebhookUrl))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "MISSING_NOTIFICATION_TARGET", Message = "At least one notification email or webhook URL is required when notifications are enabled." });
            }

            // Validate execution history retention
            if (ExecutionHistoryRetentionDays <= 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "INVALID_HISTORY_RETENTION", Message = "Execution history retention days must be greater than zero." });
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the execution statistics for a scheduled flow.
    /// </summary>
    public class ExecutionStatistics
    {
        /// <summary>
        /// Gets or sets the total number of executions.
        /// </summary>
        public int TotalExecutions { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of successful executions.
        /// </summary>
        public int SuccessfulExecutions { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of failed executions.
        /// </summary>
        public int FailedExecutions { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average execution time in milliseconds.
        /// </summary>
        public double AverageExecutionTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of records processed.
        /// </summary>
        public long TotalRecordsProcessed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of records with errors.
        /// </summary>
        public long TotalRecordsWithErrors { get; set; } = 0;

        /// <summary>
        /// Gets or sets the last execution duration in milliseconds.
        /// </summary>
        public long LastExecutionDurationMs { get; set; } = 0;
    }

    /// <summary>
    /// Represents the notification settings for a scheduled flow.
    /// </summary>
    public class ScheduledFlowNotificationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to send notifications on success.
        /// </summary>
        public bool NotifyOnSuccess { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to send notifications on failure.
        /// </summary>
        public bool NotifyOnFailure { get; set; } = true;

        /// <summary>
        /// Gets or sets the notification email addresses.
        /// </summary>
        public List<string> NotificationEmails { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the webhook URL for notifications.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include execution details in notifications.
        /// </summary>
        public bool IncludeExecutionDetails { get; set; } = true;
    }
}
