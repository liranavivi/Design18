using FlowOrchestrator.Abstractions.Statistics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowOrchestrator.Observability.Statistics.Services
{
    /// <summary>
    /// Health check for the statistics service.
    /// </summary>
    public class StatisticsServiceHealthCheck : IHealthCheck
    {
        private readonly IStatisticsLifecycle _statisticsLifecycle;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsServiceHealthCheck"/> class.
        /// </summary>
        /// <param name="statisticsLifecycle">The statistics lifecycle.</param>
        public StatisticsServiceHealthCheck(IStatisticsLifecycle statisticsLifecycle)
        {
            _statisticsLifecycle = statisticsLifecycle;
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var status = _statisticsLifecycle.GetCollectionStatus();
            
            if (status == StatisticsCollectionStatus.ERROR)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Statistics collection is in an error state"));
            }
            else if (status == StatisticsCollectionStatus.UNINITIALIZED)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Statistics collection is not initialized"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Statistics collection is {status.ToString().ToLowerInvariant()}"));
            }
        }
    }
}
