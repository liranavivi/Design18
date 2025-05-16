using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.MemoryManager.Interfaces;

namespace FlowOrchestrator.MemoryManager;

/// <summary>
/// Background service for the memory manager.
/// </summary>
public class Worker : BackgroundService
{
    private readonly IMemoryManager _memoryManager;
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _monitoringInterval;
    private readonly int _maxMemoryAgeHours;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="memoryManager">The memory manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    public Worker(
        IMemoryManager memoryManager,
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        _memoryManager = memoryManager;
        _logger = logger;
        _configuration = configuration;

        // Get configuration values
        _cleanupInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<double>("MemoryManager:CleanupIntervalMinutes", 15));

        _monitoringInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<double>("MemoryManager:MonitoringIntervalMinutes", 5));

        _maxMemoryAgeHours = _configuration.GetValue<int>("MemoryManager:MaxMemoryAgeHours", 24);
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Memory Manager Worker starting");

        try
        {
            // Initialize the memory manager
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("ServiceId", _configuration.GetValue<string>("MemoryManager:ServiceId", "MEMORY-MANAGER-SERVICE"));
            parameters.SetParameter("MaxMemoryAgeHours", _maxMemoryAgeHours);
            parameters.SetParameter("CleanupIntervalMinutes", _cleanupInterval.TotalMinutes);
            _memoryManager.Initialize(parameters);

            _logger.LogInformation("Memory Manager initialized with cleanup interval: {CleanupInterval}, monitoring interval: {MonitoringInterval}, max memory age: {MaxMemoryAgeHours} hours",
                _cleanupInterval, _monitoringInterval, _maxMemoryAgeHours);

            // Start background tasks
            var cleanupTask = RunCleanupTaskAsync(stoppingToken);
            var monitoringTask = RunMonitoringTaskAsync(stoppingToken);

            // Wait for both tasks to complete
            await Task.WhenAll(cleanupTask, monitoringTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Memory Manager Worker");
        }
        finally
        {
            // Terminate the memory manager
            _memoryManager.Terminate();
            _logger.LogInformation("Memory Manager terminated");
        }
    }

    private async Task RunCleanupTaskAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting memory cleanup task with interval: {CleanupInterval}", _cleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Running memory cleanup task");

                // In a real implementation, you would scan for expired memory allocations
                // and clean them up. This is a simplified implementation.

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in memory cleanup task");

                // Wait a short time before retrying
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task RunMonitoringTaskAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting memory monitoring task with interval: {MonitoringInterval}", _monitoringInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Running memory monitoring task");

                // Get memory statistics
                var statistics = await _memoryManager.GetMemoryStatisticsAsync();

                // Log statistics
                _logger.LogInformation("Memory statistics: {Statistics}", statistics);

                // Wait for the next monitoring interval
                await Task.Delay(_monitoringInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in memory monitoring task");

                // Wait a short time before retrying
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
