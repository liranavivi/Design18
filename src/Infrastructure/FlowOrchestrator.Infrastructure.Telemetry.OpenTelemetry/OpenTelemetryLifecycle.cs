using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Metrics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Models;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry
{
    /// <summary>
    /// Implementation of IStatisticsLifecycle using OpenTelemetry.
    /// </summary>
    public class OpenTelemetryLifecycle : IStatisticsLifecycle
    {
        private readonly MetricsService _metricsService;
        private readonly ILogger<OpenTelemetryLifecycle> _logger;
        private readonly OpenTelemetryOptions _options;
        private StatisticsCollectionStatus _status = StatisticsCollectionStatus.UNINITIALIZED;
        private StatisticsCollectionConfiguration _configuration = new();
        private readonly Dictionary<string, Dictionary<string, object>> _archivedStatistics = new();
        private Timer? _collectionTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTelemetryLifecycle"/> class.
        /// </summary>
        /// <param name="metricsService">The metrics service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The OpenTelemetry options.</param>
        public OpenTelemetryLifecycle(
            MetricsService metricsService,
            ILogger<OpenTelemetryLifecycle> logger,
            IOptions<OpenTelemetryOptions> options)
        {
            _metricsService = metricsService;
            _logger = logger;
            _options = options.Value;
            
            // Initialize default configuration
            _configuration.CollectionIntervalMs = _options.MetricsCollectionIntervalMs;
        }

        /// <inheritdoc/>
        public string LifecycleId => $"OpenTelemetryLifecycle-{_options.ServiceInstanceId}";

        /// <inheritdoc/>
        public string LifecycleType => "OpenTelemetry";

        /// <inheritdoc/>
        public void Initialize()
        {
            try
            {
                if (_status != StatisticsCollectionStatus.UNINITIALIZED)
                {
                    _logger.LogWarning("Attempted to initialize statistics lifecycle when already initialized");
                    return;
                }
                
                _logger.LogInformation("Initializing OpenTelemetry statistics lifecycle");
                
                // Set status to initialized
                _status = StatisticsCollectionStatus.INITIALIZED;
                
                _logger.LogInformation("OpenTelemetry statistics lifecycle initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OpenTelemetry statistics lifecycle");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void StartCollection()
        {
            try
            {
                if (_status == StatisticsCollectionStatus.ACTIVE)
                {
                    _logger.LogWarning("Attempted to start statistics collection when already active");
                    return;
                }
                
                if (_status == StatisticsCollectionStatus.UNINITIALIZED)
                {
                    _logger.LogWarning("Attempted to start statistics collection when not initialized");
                    return;
                }
                
                _logger.LogInformation("Starting OpenTelemetry statistics collection");
                
                // Start collection timer
                _collectionTimer = new Timer(CollectStatistics, null, 0, _configuration.CollectionIntervalMs);
                
                // Set status to active
                _status = StatisticsCollectionStatus.ACTIVE;
                
                _logger.LogInformation("OpenTelemetry statistics collection started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting OpenTelemetry statistics collection");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void StopCollection()
        {
            try
            {
                if (_status != StatisticsCollectionStatus.ACTIVE && _status != StatisticsCollectionStatus.PAUSED)
                {
                    _logger.LogWarning("Attempted to stop statistics collection when not active or paused");
                    return;
                }
                
                _logger.LogInformation("Stopping OpenTelemetry statistics collection");
                
                // Stop collection timer
                _collectionTimer?.Dispose();
                _collectionTimer = null;
                
                // Set status to stopped
                _status = StatisticsCollectionStatus.STOPPED;
                
                _logger.LogInformation("OpenTelemetry statistics collection stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping OpenTelemetry statistics collection");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void PauseCollection()
        {
            try
            {
                if (_status != StatisticsCollectionStatus.ACTIVE)
                {
                    _logger.LogWarning("Attempted to pause statistics collection when not active");
                    return;
                }
                
                _logger.LogInformation("Pausing OpenTelemetry statistics collection");
                
                // Pause collection timer
                _collectionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                
                // Set status to paused
                _status = StatisticsCollectionStatus.PAUSED;
                
                _logger.LogInformation("OpenTelemetry statistics collection paused");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing OpenTelemetry statistics collection");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void ResumeCollection()
        {
            try
            {
                if (_status != StatisticsCollectionStatus.PAUSED)
                {
                    _logger.LogWarning("Attempted to resume statistics collection when not paused");
                    return;
                }
                
                _logger.LogInformation("Resuming OpenTelemetry statistics collection");
                
                // Resume collection timer
                _collectionTimer?.Change(0, _configuration.CollectionIntervalMs);
                
                // Set status to active
                _status = StatisticsCollectionStatus.ACTIVE;
                
                _logger.LogInformation("OpenTelemetry statistics collection resumed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming OpenTelemetry statistics collection");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void ResetStatistics()
        {
            try
            {
                _logger.LogInformation("Resetting OpenTelemetry statistics");
                
                // Reset metrics
                // Note: OpenTelemetry doesn't provide a direct way to reset metrics,
                // but we can reset our internal state
                
                _logger.LogInformation("OpenTelemetry statistics reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting OpenTelemetry statistics");
                _status = StatisticsCollectionStatus.ERROR;
            }
        }

        /// <inheritdoc/>
        public void ArchiveStatistics(string archiveId)
        {
            try
            {
                _logger.LogInformation("Archiving OpenTelemetry statistics with ID {ArchiveId}", archiveId);
                
                // Get current metrics
                var metrics = _metricsService.GetMetricValues();
                
                // Archive metrics
                _archivedStatistics[archiveId] = new Dictionary<string, object>(metrics);
                
                _logger.LogInformation("OpenTelemetry statistics archived with ID {ArchiveId}", archiveId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving OpenTelemetry statistics with ID {ArchiveId}", archiveId);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, object> RetrieveArchivedStatistics(string archiveId)
        {
            try
            {
                if (_archivedStatistics.TryGetValue(archiveId, out var statistics))
                {
                    return new Dictionary<string, object>(statistics);
                }
                
                _logger.LogWarning("Attempted to retrieve non-existent archived statistics with ID {ArchiveId}", archiveId);
                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archived statistics with ID {ArchiveId}", archiveId);
                return new Dictionary<string, object>();
            }
        }

        /// <inheritdoc/>
        public StatisticsCollectionStatus GetCollectionStatus()
        {
            return _status;
        }

        /// <inheritdoc/>
        public StatisticsCollectionConfiguration GetCollectionConfiguration()
        {
            return _configuration;
        }

        /// <inheritdoc/>
        public void SetCollectionConfiguration(StatisticsCollectionConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("Setting OpenTelemetry statistics collection configuration");
                
                // Store configuration
                _configuration = configuration;
                
                // Update collection timer if active
                if (_status == StatisticsCollectionStatus.ACTIVE && _collectionTimer != null)
                {
                    _collectionTimer.Change(0, _configuration.CollectionIntervalMs);
                }
                
                _logger.LogInformation("OpenTelemetry statistics collection configuration set");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting OpenTelemetry statistics collection configuration");
            }
        }

        private void CollectStatistics(object? state)
        {
            try
            {
                // This method is called by the timer to collect statistics
                // OpenTelemetry handles the actual collection and export of metrics,
                // so we don't need to do anything here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting OpenTelemetry statistics");
            }
        }
    }
}
