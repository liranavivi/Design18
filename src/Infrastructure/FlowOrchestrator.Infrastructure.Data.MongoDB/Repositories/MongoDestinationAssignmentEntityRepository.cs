using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for destination assignment entities.
    /// </summary>
    public class MongoDestinationAssignmentEntityRepository : MongoEntityRepository<DestinationAssignmentEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDestinationAssignmentEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoDestinationAssignmentEntityRepository(MongoDbContext context, ILogger<MongoDestinationAssignmentEntityRepository> logger)
            : base(context, "destinationAssignments", logger)
        {
        }
    }
}
