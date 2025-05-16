using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Domain.Entities;
using System.Text.RegularExpressions;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Utils
{
    /// <summary>
    /// Utility class for converting between different schedule expression formats.
    /// </summary>
    public static class CronExpressionConverter
    {
        /// <summary>
        /// Converts a schedule expression to a Quartz cron expression.
        /// </summary>
        /// <param name="scheduleType">The schedule type.</param>
        /// <param name="scheduleExpression">The schedule expression.</param>
        /// <returns>The Quartz cron expression.</returns>
        public static string ToQuartzCronExpression(ScheduleType scheduleType, string scheduleExpression)
        {
            return scheduleType switch
            {
                ScheduleType.ONCE => ConvertOnceToCron(scheduleExpression),
                ScheduleType.CRON => NormalizeCronExpression(scheduleExpression),
                ScheduleType.INTERVAL => ConvertIntervalToCron(scheduleExpression),
                ScheduleType.CUSTOM => scheduleExpression, // Assume custom expressions are already in Quartz format
                _ => throw new ArgumentException($"Unsupported schedule type: {scheduleType}")
            };
        }

        /// <summary>
        /// Converts a one-time schedule expression to a cron expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression (expected to be a date/time string).</param>
        /// <returns>The cron expression.</returns>
        private static string ConvertOnceToCron(string scheduleExpression)
        {
            if (DateTime.TryParse(scheduleExpression, out DateTime dateTime))
            {
                // Format: seconds minutes hours day-of-month month day-of-week year
                return $"{dateTime.Second} {dateTime.Minute} {dateTime.Hour} {dateTime.Day} {dateTime.Month} ? {dateTime.Year}";
            }

            throw new ArgumentException($"Invalid one-time schedule expression: {scheduleExpression}");
        }

        /// <summary>
        /// Converts an interval schedule expression to a cron expression.
        /// </summary>
        /// <param name="scheduleExpression">The schedule expression (expected to be an interval in seconds).</param>
        /// <returns>The cron expression.</returns>
        private static string ConvertIntervalToCron(string scheduleExpression)
        {
            if (int.TryParse(scheduleExpression, out int intervalSeconds))
            {
                if (intervalSeconds <= 0)
                {
                    throw new ArgumentException($"Interval must be greater than 0: {intervalSeconds}");
                }

                if (intervalSeconds < 60)
                {
                    // For intervals less than a minute, use a cron expression that runs every N seconds
                    return $"0/{intervalSeconds} * * ? * *";
                }
                else if (intervalSeconds < 3600)
                {
                    // For intervals less than an hour, use a cron expression that runs every N minutes
                    int intervalMinutes = intervalSeconds / 60;
                    return $"0 0/{intervalMinutes} * ? * *";
                }
                else if (intervalSeconds < 86400)
                {
                    // For intervals less than a day, use a cron expression that runs every N hours
                    int intervalHours = intervalSeconds / 3600;
                    return $"0 0 0/{intervalHours} ? * *";
                }
                else
                {
                    // For intervals of a day or more, use a cron expression that runs every N days
                    int intervalDays = intervalSeconds / 86400;
                    return $"0 0 0 1/{intervalDays} * ?";
                }
            }

            throw new ArgumentException($"Invalid interval schedule expression: {scheduleExpression}");
        }

        /// <summary>
        /// Normalizes a cron expression to ensure it's compatible with Quartz.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <returns>The normalized cron expression.</returns>
        private static string NormalizeCronExpression(string cronExpression)
        {
            // Validate the cron expression
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                throw new ArgumentException("Cron expression cannot be null or empty");
            }

            // Check if the cron expression is valid
            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6 || parts.Length > 7)
            {
                throw new ArgumentException($"Invalid cron expression format: {cronExpression}");
            }

            // If the cron expression has 6 parts, add seconds (0) at the beginning
            if (parts.Length == 6)
            {
                return $"0 {cronExpression}";
            }

            return cronExpression;
        }
    }
}
