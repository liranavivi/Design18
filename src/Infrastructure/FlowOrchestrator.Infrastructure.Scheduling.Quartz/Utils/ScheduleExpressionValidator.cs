using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Domain.Entities;
using Quartz;
using System.Text.RegularExpressions;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Utils
{
    /// <summary>
    /// Utility class for validating schedule expressions.
    /// </summary>
    public static class ScheduleExpressionValidator
    {
        /// <summary>
        /// Validates a schedule expression.
        /// </summary>
        /// <param name="scheduleType">The schedule type.</param>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult Validate(ScheduleType scheduleType, string scheduleExpression)
        {
            return scheduleType switch
            {
                ScheduleType.ONCE => ValidateOnceExpression(scheduleExpression),
                ScheduleType.CRON => ValidateCronExpression(scheduleExpression),
                ScheduleType.INTERVAL => ValidateIntervalExpression(scheduleExpression),
                ScheduleType.CUSTOM => ValidateCustomExpression(scheduleExpression),
                _ => ValidationResult.Error($"Unsupported schedule type: {scheduleType}")
            };
        }

        /// <summary>
        /// Validates a one-time schedule expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateOnceExpression(string scheduleExpression)
        {
            if (string.IsNullOrWhiteSpace(scheduleExpression))
            {
                return ValidationResult.Error("One-time schedule expression cannot be null or empty");
            }

            if (DateTime.TryParse(scheduleExpression, out DateTime dateTime))
            {
                if (dateTime < DateTime.UtcNow)
                {
                    return ValidationResult.Error("One-time schedule expression must be in the future");
                }

                return ValidationResult.Valid();
            }

            return ValidationResult.Error($"Invalid one-time schedule expression format: {scheduleExpression}");
        }

        /// <summary>
        /// Validates a cron schedule expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateCronExpression(string scheduleExpression)
        {
            if (string.IsNullOrWhiteSpace(scheduleExpression))
            {
                return ValidationResult.Error("Cron schedule expression cannot be null or empty");
            }

            try
            {
                // Try to convert to a Quartz cron expression
                var quartzCronExpression = CronExpressionConverter.ToQuartzCronExpression(ScheduleType.CRON, scheduleExpression);

                // Check if the cron expression is valid using Quartz
                var cronExpression = new CronExpression(quartzCronExpression);
                return ValidationResult.Valid();
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"Invalid cron schedule expression: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates an interval schedule expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateIntervalExpression(string scheduleExpression)
        {
            if (string.IsNullOrWhiteSpace(scheduleExpression))
            {
                return ValidationResult.Error("Interval schedule expression cannot be null or empty");
            }

            if (int.TryParse(scheduleExpression, out int intervalSeconds))
            {
                if (intervalSeconds <= 0)
                {
                    return ValidationResult.Error($"Interval must be greater than 0: {intervalSeconds}");
                }

                return ValidationResult.Valid();
            }

            return ValidationResult.Error($"Invalid interval schedule expression format: {scheduleExpression}");
        }

        /// <summary>
        /// Validates a custom schedule expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateCustomExpression(string scheduleExpression)
        {
            if (string.IsNullOrWhiteSpace(scheduleExpression))
            {
                return ValidationResult.Error("Custom schedule expression cannot be null or empty");
            }

            // For custom expressions, we'll just check if it's a valid cron expression
            return ValidateCronExpression(scheduleExpression);
        }
    }
}
