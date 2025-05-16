using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for source entities.
    /// </summary>
    public class MongoSourceEntityRepository : MongoEntityRepository<FileSourceEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoSourceEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoSourceEntityRepository(MongoDbContext context, ILogger<MongoSourceEntityRepository> logger)
            : base(context, "sources", logger)
        {
        }
    }
}
