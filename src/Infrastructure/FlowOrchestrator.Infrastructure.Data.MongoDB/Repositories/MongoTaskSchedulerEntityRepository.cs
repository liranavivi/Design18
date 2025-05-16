using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories
{
    /// <summary>
    /// MongoDB repository for task scheduler entities.
    /// </summary>
    public class MongoTaskSchedulerEntityRepository : MongoEntityRepository<TaskSchedulerEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoTaskSchedulerEntityRepository"/> class.
        /// </summary>
        /// <param name="context">The MongoDB context.</param>
        /// <param name="logger">The logger.</param>
        public MongoTaskSchedulerEntityRepository(MongoDbContext context, ILogger<MongoTaskSchedulerEntityRepository> logger)
            : base(context, "taskSchedulers", logger)
        {
        }
    }
}
