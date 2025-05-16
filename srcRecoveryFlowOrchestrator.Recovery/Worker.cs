using FlowOrchestrator.Recovery.Services;
using FlowOrchestrator.Recovery.Strategies;

namespace FlowOrchestrator.Recovery;

/// <summary>
/// Worker service for the FlowOrchestrator.Recovery service.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RecoveryService _recoveryService;
    private readonly ErrorClassificationService _errorClassificationService;
    private readonly IEnumerable<IRecoveryStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="recoveryService">The recovery service.</param>
    /// <param name="errorClassificationService">The error classification service.</param>
    /// <param name="strategies">The recovery strategies.</param>
    public Worker(
        ILogger<Worker> logger,
        RecoveryService recoveryService,
        ErrorClassificationService errorClassificationService,
        IEnumerable<IRecoveryStrategy> strategies)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _recoveryService = recoveryService ?? throw new ArgumentNullException(nameof(recoveryService));
        _errorClassificationService = errorClassificationService ?? throw new ArgumentNullException(nameof(errorClassificationService));
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    /// <summary>
    /// Executes the worker.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlowOrchestrator.Recovery worker service started");
        _logger.LogInformation("Registered recovery strategies: {StrategyCount}", _strategies.Count());

        foreach (var strategy in _strategies)
        {
            _logger.LogInformation("Recovery strategy: {StrategyName} - {StrategyDescription}",
                strategy.Name, strategy.Description);
        }

        try
        {
            // The worker service is primarily message-driven, so we just need to keep it alive
            while (!stoppingToken.IsCancellationRequested)
            {
                // Periodically log statistics
                LogStatistics();

                // Wait for a while
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
            _logger.LogInformation("FlowOrchestrator.Recovery worker service shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FlowOrchestrator.Recovery worker service");
        }
        finally
        {
            _logger.LogInformation("FlowOrchestrator.Recovery worker service stopped");
        }
    }

    private void LogStatistics()
    {
        try
        {
            // Log active recoveries
            var activeRecoveries = _recoveryService.GetActiveRecoveries().ToList();
            _logger.LogInformation("Active recoveries: {ActiveRecoveryCount}", activeRecoveries.Count);

            // Log error patterns
            var errorPatterns = _errorClassificationService.GetErrorPatterns().ToList();
            _logger.LogInformation("Error patterns: {ErrorPatternCount}", errorPatterns.Count);

            // Log top error patterns by frequency
            var topErrorPatterns = errorPatterns
                .OrderByDescending(p => p.FrequencyPerHour)
                .Take(5)
                .ToList();

            foreach (var pattern in topErrorPatterns)
            {
                _logger.LogInformation(
                    "Error pattern: {ServiceId}:{ErrorCode} - {OccurrenceCount} occurrences, {FrequencyPerHour:F2} per hour",
                    pattern.ServiceId, pattern.ErrorCode, pattern.OccurrenceCount, pattern.FrequencyPerHour);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging statistics");
        }
    }
}
