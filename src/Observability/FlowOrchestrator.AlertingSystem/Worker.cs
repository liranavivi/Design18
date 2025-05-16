using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Observability.Alerting.Configuration;
using FlowOrchestrator.Observability.Alerting.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FlowOrchestrator.Observability.Alerting;

/// <summary>
/// Background service for the alerting system.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AlertRuleEngineService _alertRuleEngineService;
    private readonly NotificationService _notificationService;
    private readonly AlertHistoryService _alertHistoryService;
    private readonly MetricsCollectorService _metricsCollectorService;
    private readonly IMessageBus _messageBus;
    private readonly IOptions<AlertingSystemOptions> _options;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly HealthCheckService _healthCheckService;
    private readonly TimeSpan _healthCheckInterval;
    private readonly TimeSpan _alertCheckInterval;
    private readonly Activity _serviceActivity;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    public Worker(
        ILogger<Worker> logger,
        AlertRuleEngineService alertRuleEngineService,
        NotificationService notificationService,
        AlertHistoryService alertHistoryService,
        MetricsCollectorService metricsCollectorService,
        IMessageBus messageBus,
        IOptions<AlertingSystemOptions> options,
        IHostApplicationLifetime applicationLifetime,
        HealthCheckService healthCheckService)
    {
        _logger = logger;
        _alertRuleEngineService = alertRuleEngineService;
        _notificationService = notificationService;
        _alertHistoryService = alertHistoryService;
        _metricsCollectorService = metricsCollectorService;
        _messageBus = messageBus;
        _options = options;
        _applicationLifetime = applicationLifetime;
        _healthCheckService = healthCheckService;

        _healthCheckInterval = TimeSpan.FromSeconds(options.Value.HealthCheckIntervalSeconds);
        _alertCheckInterval = TimeSpan.FromSeconds(options.Value.AlertCheckIntervalSeconds);

        // Create activity source for telemetry
        var activitySource = new ActivitySource("FlowOrchestrator.AlertingSystem");
        _serviceActivity = activitySource.StartActivity("AlertingSystemWorker")!;
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
            _logger.LogInformation("Starting alerting system worker");

            // Initialize services
            await InitializeServicesAsync();

            // Start periodic health check
            _ = RunPeriodicHealthCheckAsync(stoppingToken);

            // Main service loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check alert rules
                    await CheckAlertRulesAsync();

                    // Wait for the next check interval
                    await Task.Delay(_alertCheckInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking alert rules");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
            _logger.LogInformation("Alerting system worker shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in alerting system worker");
            _applicationLifetime.StopApplication();
        }
        finally
        {
            _serviceActivity?.Dispose();
            _logger.LogInformation("Alerting system worker stopped");
        }
    }

    private async Task InitializeServicesAsync()
    {
        _logger.LogInformation("Initializing alerting system services");

        // Initialize services
        await _alertRuleEngineService.InitializeAsync();
        await _notificationService.InitializeAsync();
        await _alertHistoryService.InitializeAsync();
        await _metricsCollectorService.InitializeAsync();

        _logger.LogInformation("Alerting system services initialized");
    }

    private async Task CheckAlertRulesAsync()
    {
        using var activity = _serviceActivity?.Source.StartActivity("CheckAlertRules");

        _logger.LogDebug("Checking alert rules");

        // Collect metrics
        var metrics = await _metricsCollectorService.CollectMetricsAsync();

        // Evaluate alert rules
        var alerts = await _alertRuleEngineService.EvaluateRulesAsync(metrics);

        if (alerts.Any())
        {
            _logger.LogInformation("Generated {AlertCount} alerts", alerts.Count);

            // Process alerts
            foreach (var alert in alerts)
            {
                // Save alert to history
                await _alertHistoryService.SaveAlertAsync(alert);

                // Send notifications
                await _notificationService.SendNotificationsAsync(alert);
            }
        }
    }

    private async Task RunPeriodicHealthCheckAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync(stoppingToken);

                if (healthReport.Status == HealthStatus.Healthy)
                {
                    _logger.LogDebug("Health check passed");
                }
                else
                {
                    _logger.LogWarning("Health check failed: {Status}", healthReport.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running health check");
            }

            await Task.Delay(_healthCheckInterval, stoppingToken);
        }
    }
}
