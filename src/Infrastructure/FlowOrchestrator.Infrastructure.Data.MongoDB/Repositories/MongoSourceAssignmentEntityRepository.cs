using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for source assignment entities.
    /// </summary>
    public class MongoSourceAssignmentEntityRepository : MongoEntityRepository<SourceAssignmentEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoSourceAssignmentEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoSourceAssignmentEntityRepository(MongoDbContext context, ILogger<MongoSourceAssignmentEntityRepository> logger)
            : base(context, "sourceAssignments", logger)
        {
        }
    }
}
