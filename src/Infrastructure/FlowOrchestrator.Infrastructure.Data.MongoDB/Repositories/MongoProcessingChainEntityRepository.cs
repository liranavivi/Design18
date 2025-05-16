using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for processing chain entities.
    /// </summary>
    public class MongoProcessingChainEntityRepository : MongoEntityRepository<ProcessingChainEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoProcessingChainEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoProcessingChainEntityRepository(MongoDbContext context, ILogger<MongoProcessingChainEntityRepository> logger)
            : base(context, "processingChains", logger)
        {
        }
    }
}
