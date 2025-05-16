using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace FlowOrchestrator.Infrastructure.Scheduling.Quartz.Services
{
    /// <summary>
    /// Factory for creating Quartz jobs.
    /// </summary>
    public class QuartzJobFactory : IQuartzJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuartzJobFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuartzJobFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public QuartzJobFactory(IServiceProvider serviceProvider, ILogger<QuartzJobFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new instance of a job.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle from which to create the job.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns>The new job instance.</returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobType = bundle.JobDetail.JobType;
                _logger.LogDebug("Creating new job of type {JobType}", jobType.FullName);

                // Create a scope for the job
                var scope = _serviceProvider.CreateScope();

                // Get the job instance from the service provider
                var job = (IJob)scope.ServiceProvider.GetRequiredService(jobType);

                // If the job implements IDisposable, dispose it when the scope is disposed
                if (job is IDisposable disposableJob)
                {
                    // Register the job for disposal when the scope is disposed
                    scope.ServiceProvider.GetRequiredService<ILogger<QuartzJobFactory>>()
                        .LogDebug("Job {JobType} implements IDisposable, will be disposed when scope is disposed", jobType.FullName);
                }

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job of type {JobType}", bundle.JobDetail.JobType.FullName);
                throw new SchedulerException($"Error creating job of type {bundle.JobDetail.JobType.FullName}", ex);
            }
        }

        /// <summary>
        /// Destroys a job instance.
        /// </summary>
        /// <param name="job">The job to destroy.</param>
        public void ReturnJob(IJob job)
        {
            // If the job implements IDisposable, dispose it
            if (job is IDisposable disposableJob)
            {
                try
                {
                    _logger.LogDebug("Disposing job of type {JobType}", job.GetType().FullName);
                    disposableJob.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing job of type {JobType}", job.GetType().FullName);
                }
            }
        }
    }
}
