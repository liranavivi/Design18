using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Jobs;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Models;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Extensions
{
    /// <summary>
    /// Extension methods for configuring Quartz services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class QuartzServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Quartz scheduling services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddQuartzScheduling(this IServiceCollection services)
        {
            return services.AddQuartzScheduling(options => { });
        }

        /// <summary>
        /// Adds Quartz scheduling services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddQuartzScheduling(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<QuartzConfiguration>(options =>
                configuration.GetSection("Quartz").Bind(options));
            return services.AddQuartzScheduling();
        }

        /// <summary>
        /// Adds Quartz scheduling services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddQuartzScheduling(this IServiceCollection services, Action<QuartzConfiguration> configureOptions)
        {
            // Configure options
            services.Configure(configureOptions);

            // Register job factory
            services.AddSingleton<IQuartzJobFactory, QuartzJobFactory>();

            // Register scheduler service
            services.AddSingleton<IQuartzSchedulerService, QuartzSchedulerService>();

            // Register jobs
            services.AddTransient<FlowExecutionJob>();
            services.AddTransient<TaskSchedulerJob>();

            return services;
        }
    }
}
