using FlowOrchestrator.BranchController.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace FlowOrchestrator.BranchController;

/// <summary>
/// Background service for the branch controller.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly BranchContextService _branchContextService;
    private readonly BranchIsolationService _branchIsolationService;
    private readonly BranchCompletionService _branchCompletionService;
    private readonly TelemetryService _telemetryService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly HealthCheckService _healthCheckService;
    private readonly TimeSpan _healthCheckInterval = TimeSpan.FromMinutes(1);
    private readonly Activity _serviceActivity;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="branchContextService">The branch context service.</param>
    /// <param name="branchIsolationService">The branch isolation service.</param>
    /// <param name="branchCompletionService">The branch completion service.</param>
    /// <param name="telemetryService">The telemetry service.</param>
    /// <param name="applicationLifetime">The application lifetime.</param>
    /// <param name="healthCheckService">The health check service.</param>
    public Worker(
        ILogger<Worker> logger,
        BranchContextService branchContextService,
        BranchIsolationService branchIsolationService,
        BranchCompletionService branchCompletionService,
        TelemetryService telemetryService,
        IHostApplicationLifetime applicationLifetime,
        HealthCheckService healthCheckService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
        _branchIsolationService = branchIsolationService ?? throw new ArgumentNullException(nameof(branchIsolationService));
        _branchCompletionService = branchCompletionService ?? throw new ArgumentNullException(nameof(branchCompletionService));
        _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));

        // Create service activity for telemetry
        _serviceActivity = new Activity("BranchControllerService");
        _serviceActivity.SetTag("component", "BranchController");
        _serviceActivity.SetTag("serviceType", "Worker");
        _serviceActivity.Start();
    }

    /// <summary>
    /// Executes the worker.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Branch Controller service starting");

        try
        {
            // Run health checks periodically
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Perform health check
                    var healthReport = await _healthCheckService.CheckHealthAsync(stoppingToken);

                    if (healthReport.Status == HealthStatus.Healthy)
                    {
                        _logger.LogInformation("Health check passed");
                    }
                    else
                    {
                        _logger.LogWarning("Health check failed with status {Status}", healthReport.Status);

                        // Log detailed health check results
                        foreach (var entry in healthReport.Entries)
                        {
                            _logger.LogWarning("Health check {Name} status: {Status}, description: {Description}",
                                entry.Key, entry.Value.Status, entry.Value.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing health check");
                }

                await Task.Delay(_healthCheckInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown, no need to log an error
            _logger.LogInformation("Branch Controller service shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in Branch Controller service");

            // Stop the application on unhandled exception
            _applicationLifetime.StopApplication();
        }
        finally
        {
            // Stop the service activity
            _serviceActivity.Stop();
        }
    }
}
