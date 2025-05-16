using FlowOrchestrator.Management.Scheduling.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Management.Scheduling
{
    /// <summary>
    /// Background service for the task scheduler.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SchedulerManager _schedulerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="schedulerManager">The scheduler manager.</param>
        public Worker(
            ILogger<Worker> logger,
            SchedulerManager schedulerManager)
        {
            _logger = logger;
            _schedulerManager = schedulerManager;
        }

        /// <summary>
        /// Executes the background service.
        /// </summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting task scheduler worker");

                // Initialize the scheduler
                await _schedulerManager.InitializeAsync();

                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in task scheduler worker");
                throw;
            }
            finally
            {
                // Shutdown the scheduler
                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await _schedulerManager.ShutdownAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error shutting down scheduler");
                    }
                }
            }
        }

        /// <summary>
        /// Called when the service is stopping.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping task scheduler worker");

            try
            {
                // Shutdown the scheduler
                await _schedulerManager.ShutdownAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down scheduler");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
