using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for flow entities.
    /// </summary>
    public class MongoFlowEntityRepository : MongoEntityRepository<FlowEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoFlowEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoFlowEntityRepository(MongoDbContext context, ILogger<MongoFlowEntityRepository> logger)
            : base(context, "flows", logger)
        {
        }
    }
}
