using FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Cluster;
using FlowOrchestrator.Infrastructure.Data.Hazelcast.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Extensions
{
    /// <summary>
    /// Extension methods for registering Hazelcast services with the dependency injection container.
    /// </summary>
    public static class HazelcastServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Hazelcast services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcast(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure options
            services.Configure<HazelcastOptions>(configuration.GetSection("Hazelcast"));
            
            // Register core services
            services.AddSingleton<HazelcastContext>();
            services.AddSingleton<ClusterManager>();
            
            // Register cache services
            services.AddSingleton<IDistributedCache>(provider => 
            {
                var context = provider.GetRequiredService<HazelcastContext>();
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<HazelcastDistributedCache>>();
                return new HazelcastDistributedCache(context, logger);
            });
            
            return services;
        }

        /// <summary>
        /// Adds Hazelcast services to the service collection with the specified options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcast(this IServiceCollection services, Action<HazelcastOptions> configureOptions)
        {
            // Configure options
            services.Configure(configureOptions);
            
            // Register core services
            services.AddSingleton<HazelcastContext>();
            services.AddSingleton<ClusterManager>();
            
            // Register cache services
            services.AddSingleton<IDistributedCache>(provider => 
            {
                var context = provider.GetRequiredService<HazelcastContext>();
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<HazelcastDistributedCache>>();
                return new HazelcastDistributedCache(context, logger);
            });
            
            return services;
        }

        /// <summary>
        /// Adds a Hazelcast repository to the service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <typeparam name="TRepository">The type of repository.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="mapName">The map name.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcastRepository<TEntity, TRepository>(
            this IServiceCollection services, 
            string mapName)
            where TEntity : class
            where TRepository : class, Repositories.IHazelcastRepository<TEntity>
        {
            services.AddSingleton<Repositories.IHazelcastRepository<TEntity>>(provider => 
            {
                var context = provider.GetRequiredService<HazelcastContext>();
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TRepository>>();
                return (TRepository)Activator.CreateInstance(typeof(TRepository), context, logger, mapName)!;
            });
            
            return services;
        }

        /// <summary>
        /// Adds a Hazelcast entity repository to the service collection.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <typeparam name="TRepository">The type of repository.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="mapName">The map name.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcastEntityRepository<TEntity, TRepository>(
            this IServiceCollection services, 
            string mapName)
            where TEntity : class, FlowOrchestrator.Abstractions.Entities.IEntity
            where TRepository : class, Repositories.IHazelcastEntityRepository<TEntity>
        {
            services.AddSingleton<Repositories.IHazelcastEntityRepository<TEntity>>(provider => 
            {
                var context = provider.GetRequiredService<HazelcastContext>();
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TRepository>>();
                return (TRepository)Activator.CreateInstance(typeof(TRepository), context, logger, mapName)!;
            });
            
            return services;
        }
    }
}
