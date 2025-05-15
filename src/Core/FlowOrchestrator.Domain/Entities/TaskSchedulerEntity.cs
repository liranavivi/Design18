using FlowOrchestrator.Abstractions.Common;
using System.Text.RegularExpressions;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractTaskSchedulerEntity class.
    /// Defines an active scheduling component.
    /// </summary>
    public class TaskSchedulerEntity : AbstractTaskSchedulerEntity
    {
        /// <summary>
        /// Gets or sets the scheduler type.
        /// </summary>
        public string SchedulerType { get; set; } = "CRON";

        /// <summary>
        /// Gets or sets the maximum number of concurrent executions.
        /// </summary>
        public int MaxConcurrentExecutions { get; set; } = 1;

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
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();

        /// <summary>
        /// Gets or sets the execution history retention days.
        /// </summary>
        public int ExecutionHistoryRetentionDays { get; set; } = 30;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntity"/> class.
        /// </summary>
        public TaskSchedulerEntity()
        {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntity"/> class with the specified scheduler ID.
        /// </summary>
        /// <param name="schedulerId">The scheduler identifier.</param>
        public TaskSchedulerEntity(string schedulerId)
        {
            SchedulerId = schedulerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntity"/> class with the specified scheduler ID and name.
        /// </summary>
        /// <param name="schedulerId">The scheduler identifier.</param>
        /// <param name="name">The name of the scheduler.</param>
        public TaskSchedulerEntity(string schedulerId, string name) : this(schedulerId)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntity"/> class with the specified scheduler ID, name, and schedule type.
        /// </summary>
        /// <param name="schedulerId">The scheduler identifier.</param>
        /// <param name="name">The name of the scheduler.</param>
        /// <param name="scheduleType">The schedule type.</param>
        /// <param name="scheduleExpression">The schedule expression.</param>
        public TaskSchedulerEntity(string schedulerId, string name, ScheduleType scheduleType, string scheduleExpression) : this(schedulerId, name)
        {
            ScheduleType = scheduleType;
            ScheduleExpression = scheduleExpression;
        }

        /// <summary>
        /// Validates the schedule expression based on the schedule type.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateScheduleExpression()
        {
            var result = new ValidationResult { IsValid = true };

            switch (ScheduleType)
            {
                case ScheduleType.ONCE:
                    // For one-time schedules, the expression should be a valid DateTime
                    if (!DateTime.TryParse(ScheduleExpression, out _))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Code = "INVALID_DATETIME_FORMAT", Message = "Schedule expression must be a valid date and time for one-time schedules." });
                    }
                    break;

                case ScheduleType.CRON:
                    // For cron schedules, validate the cron expression format
                    if (!IsValidCronExpression(ScheduleExpression))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Code = "INVALID_CRON_EXPRESSION", Message = "Invalid cron expression format." });
                    }
                    break;

                case ScheduleType.INTERVAL:
                    // For interval schedules, the expression should be a valid integer (seconds)
                    if (!int.TryParse(ScheduleExpression, out int intervalSeconds) || intervalSeconds <= 0)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Code = "INVALID_INTERVAL", Message = "Schedule expression must be a positive integer representing seconds for interval schedules." });
                    }
                    break;

                case ScheduleType.CUSTOM:
                    // For custom schedules, we would need to validate based on the specific custom format
                    // This is a placeholder for custom validation logic
                    if (string.IsNullOrWhiteSpace(ScheduleExpression))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Code = "EMPTY_CUSTOM_EXPRESSION", Message = "Custom schedule expression cannot be empty." });
                    }
                    break;

                default:
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "UNSUPPORTED_SCHEDULE_TYPE", Message = $"Schedule type '{ScheduleType}' is not supported." });
                    break;
            }

            return result;
        }

        /// <summary>
        /// Calculates the next execution time based on the schedule expression.
        /// </summary>
        /// <returns>The next execution time.</returns>
        public override DateTime? CalculateNextExecutionTime()
        {
            // If the scheduler is not enabled or has ended, return null
            if (!IsEnabled || (EndDateTime.HasValue && EndDateTime.Value < DateTime.UtcNow))
            {
                return null;
            }

            // If the scheduler has not started yet, return the start date/time
            if (StartDateTime.HasValue && StartDateTime.Value > DateTime.UtcNow)
            {
                return StartDateTime.Value;
            }

            // Calculate the next execution time based on the schedule type
            switch (ScheduleType)
            {
                case ScheduleType.ONCE:
                    // For one-time schedules, if it's in the future, return it; otherwise, return null
                    if (DateTime.TryParse(ScheduleExpression, out DateTime onceDateTime))
                    {
                        return onceDateTime > DateTime.UtcNow ? onceDateTime : null;
                    }
                    return null;

                case ScheduleType.CRON:
                    // For cron schedules, calculate the next occurrence
                    // This is a simplified implementation; in a real system, you would use a cron parser library
                    return CalculateNextCronOccurrence(ScheduleExpression);

                case ScheduleType.INTERVAL:
                    // For interval schedules, add the interval to the last execution time or now
                    if (int.TryParse(ScheduleExpression, out int intervalSeconds))
                    {
                        DateTime baseTime = LastExecutionTime ?? DateTime.UtcNow;
                        return baseTime.AddSeconds(intervalSeconds);
                    }
                    return null;

                case ScheduleType.CUSTOM:
                    // For custom schedules, this would depend on the specific implementation
                    // This is a placeholder for custom calculation logic
                    return DateTime.UtcNow.AddHours(1); // Default to 1 hour from now

                default:
                    return null;
            }
        }

        /// <summary>
        /// Determines whether the specified cron expression is valid.
        /// </summary>
        /// <param name="cronExpression">The cron expression to validate.</param>
        /// <returns>True if the cron expression is valid, otherwise false.</returns>
        private bool IsValidCronExpression(string cronExpression)
        {
            // This is a simplified cron expression validator
            // In a real implementation, you would use a more comprehensive validation
            
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                return false;
            }

            // Basic pattern: seconds minutes hours day-of-month month day-of-week [year]
            // This regex is a simplified version and doesn't cover all valid cron expressions
            var cronPattern = @"^(\*|[0-9,\-*/]+)\s+(\*|[0-9,\-*/]+)\s+(\*|[0-9,\-*/]+)\s+(\*|[0-9,\-?/LW]+)\s+(\*|[0-9,\-*/]+|JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)\s+(\*|[0-9,\-*/]+|SUN|MON|TUE|WED|THU|FRI|SAT)(\s+(\*|[0-9,\-*/]+))?$";
            
            return Regex.IsMatch(cronExpression, cronPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Calculates the next occurrence of a cron expression.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <returns>The next occurrence date and time.</returns>
        private DateTime? CalculateNextCronOccurrence(string cronExpression)
        {
            // This is a placeholder for cron calculation logic
            // In a real implementation, you would use a cron parser library like Quartz.NET or Cronos
            
            // For this example, we'll just return a time 1 day from now
            return DateTime.UtcNow.AddDays(1);
        }
    }

    /// <summary>
    /// Represents the notification settings for a task scheduler.
    /// </summary>
    public class NotificationSettings
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
    }
}
