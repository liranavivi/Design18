using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for scheduled flow entities.
    /// </summary>
    public class MongoScheduledFlowEntityRepository : MongoEntityRepository<ScheduledFlowEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoScheduledFlowEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoScheduledFlowEntityRepository(MongoDbContext context, ILogger<MongoScheduledFlowEntityRepository> logger)
            : base(context, "scheduledFlows", logger)
        {
        }
    }
}
