using FlowOrchestrator.Domain.Entities;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Configuration;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Mapping;
using FlowOrchestrator.Infrastructure.Data.MongoDB.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Extensions
{
    /// <summary>
    /// Extension methods for registering MongoDB services with the dependency injection container.
    /// </summary>
    public static class MongoDbServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MongoDB services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            // Register MongoDB options
            services.Configure<MongoDbOptions>(configuration.GetSection("MongoDB"));

            // Register MongoDB context
            services.AddSingleton<MongoDbContext>();

            // Register entity mappings
            services.AddSingleton<FlowEntityMap>();
            services.AddSingleton<ImporterServiceEntityMap>();

            // Register repositories
            services.AddScoped<IEntityRepository<FlowEntity>, MongoFlowEntityRepository>();
            services.AddScoped<IEntityRepository<FileSourceEntity>, MongoSourceEntityRepository>();
            services.AddScoped<IEntityRepository<FileDestinationEntity>, MongoDestinationEntityRepository>();
            services.AddScoped<IEntityRepository<ProcessingChainEntity>, MongoProcessingChainEntityRepository>();
            services.AddScoped<IEntityRepository<SourceAssignmentEntity>, MongoSourceAssignmentEntityRepository>();
            services.AddScoped<IEntityRepository<DestinationAssignmentEntity>, MongoDestinationAssignmentEntityRepository>();
            services.AddScoped<IEntityRepository<ScheduledFlowEntity>, MongoScheduledFlowEntityRepository>();
            services.AddScoped<IEntityRepository<TaskSchedulerEntity>, MongoTaskSchedulerEntityRepository>();
            services.AddScoped<IEntityRepository<ImporterServiceEntity>, MongoImporterServiceEntityRepository>();

            return services;
        }
    }
}
