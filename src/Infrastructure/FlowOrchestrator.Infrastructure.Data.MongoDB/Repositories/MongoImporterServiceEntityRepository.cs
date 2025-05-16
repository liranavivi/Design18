using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for importer service entities.
    /// </summary>
    public class MongoImporterServiceEntityRepository : MongoEntityRepository<ImporterServiceEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoImporterServiceEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoImporterServiceEntityRepository(MongoDbContext context, ILogger<MongoImporterServiceEntityRepository> logger)
            : base(context, "importerServices", logger)
        {
        }
    }
}
