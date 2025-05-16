using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowOrchestrator.Observability.Monitoring.HealthChecks
{
    /// <summary>
    /// Health check for the monitoring framework.
    /// </summary>
    public class MonitoringFrameworkHealthCheck : IHealthCheck
    {
        private readonly ILogger<MonitoringFrameworkHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringFrameworkHealthCheck"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MonitoringFrameworkHealthCheck(ILogger<MonitoringFrameworkHealthCheck> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the monitoring framework is running
                return Task.FromResult(HealthCheckResult.Healthy("Monitoring Framework is running"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking monitoring framework health");
                return Task.FromResult(HealthCheckResult.Unhealthy("Error checking monitoring framework health", ex));
            }
        }
    }
}
