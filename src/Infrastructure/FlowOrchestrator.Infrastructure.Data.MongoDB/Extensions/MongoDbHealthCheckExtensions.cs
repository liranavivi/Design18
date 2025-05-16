using FlowOrchestrator.Infrastructure.Data.MongoDB.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Extensions
{
    /// <summary>
    /// Extension methods for registering MongoDB health checks.
    /// </summary>
    public static class MongoDbHealthCheckExtensions
    {
        /// <summary>
        /// Adds a MongoDB health check to the specified <see cref="IHealthChecksBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the health check to.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddMongoDbHealthCheck(
            this IHealthChecksBuilder builder,
            string name = "mongodb",
            HealthStatus? failureStatus = default,
            string[]? tags = null,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? "mongodb",
                sp => new MongoDbHealthCheck(
                    sp.GetRequiredService<Configuration.MongoDbContext>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MongoDbHealthCheck>>()),
                failureStatus,
                tags,
                timeout));
        }
    }
}
