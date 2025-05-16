using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Response model for health check endpoints.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the overall health status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the health check duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the individual health check results.
        /// </summary>
        public List<HealthCheckEntry> Checks { get; set; } = new List<HealthCheckEntry>();

        /// <summary>
        /// Gets or sets the timestamp of the health check.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a health check response from a health report.
        /// </summary>
        /// <param name="report">The health report.</param>
        /// <returns>The health check response.</returns>
        public static HealthCheckResponse FromReport(HealthReport report)
        {
            return new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Checks = report.Entries.Select(entry => new HealthCheckEntry
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description ?? string.Empty,
                    Duration = entry.Value.Duration
                }).ToList(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Individual health check entry.
    /// </summary>
    public class HealthCheckEntry
    {
        /// <summary>
        /// Gets or sets the name of the health check.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the health check.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the health check.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of the health check.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
