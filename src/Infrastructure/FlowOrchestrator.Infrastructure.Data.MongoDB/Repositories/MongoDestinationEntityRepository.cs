using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for destination entities.
    /// </summary>
    public class MongoDestinationEntityRepository : MongoEntityRepository<FileDestinationEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDestinationEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoDestinationEntityRepository(MongoDbContext context, ILogger<MongoDestinationEntityRepository> logger)
            : base(context, "destinations", logger)
        {
        }
    }
}
