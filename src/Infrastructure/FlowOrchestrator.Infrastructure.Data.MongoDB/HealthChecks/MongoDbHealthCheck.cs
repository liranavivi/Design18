using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.HealthChecks
{
    /// <summary>
    /// Health check for MongoDB connection.
    /// </summary>
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<MongoDbHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbHealthCheck"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoDbHealthCheck(MongoDbContext context, ILogger<MongoDbHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Checks the health of the MongoDB connection.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The health check result.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ping the database
                await _context.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy("MongoDB connection is healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB health check failed");
                return HealthCheckResult.Unhealthy("MongoDB connection is unhealthy", ex);
            }
        }
    }
}
