using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Recovery.Models;
using FlowOrchestrator.Recovery.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Recovery.Services
{
    /// <summary>
    /// Service for handling error recovery and resilience.
    /// </summary>
    public class RecoveryService
    {
        private readonly ILogger<RecoveryService> _logger;
        private readonly IEnumerable<IRecoveryStrategy> _strategies;
        private readonly RecoveryServiceOptions _options;
        private readonly ConcurrentDictionary<string, RecoveryContext> _activeRecoveries = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="strategies">The recovery strategies.</param>
        /// <param name="options">The recovery service options.</param>
        public RecoveryService(
            ILogger<RecoveryService> logger,
            IEnumerable<IRecoveryStrategy> strategies,
            IOptions<RecoveryServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _logger.LogInformation("Recovery service initialized with {StrategyCount} strategies", _strategies.Count());
        }

        /// <summary>
        /// Handles an error and applies the appropriate recovery strategy.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <param name="executionContext">The execution context.</param>
        /// <returns>The recovery result.</returns>
        public RecoveryResult HandleError(ErrorContext errorContext, FlowOrchestrator.Abstractions.Common.ExecutionContext? executionContext = null)
        {
            _logger.LogInformation("Handling error {ErrorCode} from service {ServiceId}",
                errorContext.ErrorCode, errorContext.ServiceId);

            // Create a recovery context
            var recoveryContext = new RecoveryContext
            {
                ErrorContext = errorContext,
                ExecutionContext = executionContext,
                RecoveryId = Guid.NewGuid().ToString(),
                StartTimestamp = DateTime.UtcNow
            };

            // Store the recovery context
            _activeRecoveries[recoveryContext.RecoveryId] = recoveryContext;

            try
            {
                // Find applicable strategies
                var applicableStrategies = _strategies
                    .Where(s => s.IsApplicable(errorContext))
                    .ToList();

                _logger.LogDebug("Found {StrategyCount} applicable strategies for error {ErrorCode}",
                    applicableStrategies.Count, errorContext.ErrorCode);

                if (applicableStrategies.Count == 0)
                {
                    _logger.LogWarning("No applicable recovery strategies found for error {ErrorCode}",
                        errorContext.ErrorCode);

                    return RecoveryResult.Failed(
                        RecoveryAction.FAIL_BRANCH,
                        "No applicable recovery strategies found",
                        recoveryContext,
                        errorContext);
                }

                // Apply the first applicable strategy
                // In a more sophisticated implementation, we could have a strategy selection algorithm
                var strategy = applicableStrategies.First();
                recoveryContext.StrategyName = strategy.Name;

                _logger.LogInformation("Applying recovery strategy {StrategyName} for error {ErrorCode}",
                    strategy.Name, errorContext.ErrorCode);

                // Execute the strategy
                var result = strategy.Recover(recoveryContext);

                // Log the result
                if (result.Success)
                {
                    _logger.LogInformation("Recovery strategy {StrategyName} succeeded for error {ErrorCode}: {Message}",
                        strategy.Name, errorContext.ErrorCode, result.Message);
                }
                else
                {
                    _logger.LogWarning("Recovery strategy {StrategyName} failed for error {ErrorCode}: {Message}",
                        strategy.Name, errorContext.ErrorCode, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing recovery strategy for error {ErrorCode}",
                    errorContext.ErrorCode);

                return RecoveryResult.Failed(
                    RecoveryAction.FAIL_EXECUTION,
                    $"Error executing recovery strategy: {ex.Message}",
                    recoveryContext,
                    errorContext);
            }
            finally
            {
                // Clean up old recoveries
                CleanupOldRecoveries();
            }
        }

        /// <summary>
        /// Gets the recovery context for the specified recovery identifier.
        /// </summary>
        /// <param name="recoveryId">The recovery identifier.</param>
        /// <returns>The recovery context, or null if not found.</returns>
        public RecoveryContext? GetRecoveryContext(string recoveryId)
        {
            return _activeRecoveries.TryGetValue(recoveryId, out var context) ? context : null;
        }

        /// <summary>
        /// Gets all active recovery contexts.
        /// </summary>
        /// <returns>The active recovery contexts.</returns>
        public IEnumerable<RecoveryContext> GetActiveRecoveries()
        {
            return _activeRecoveries.Values;
        }

        /// <summary>
        /// Classifies an exception into an error context.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <returns>The error context.</returns>
        public ErrorContext ClassifyException(
            Exception ex,
            string serviceId,
            string? executionId = null,
            string? branchId = null,
            string? stepId = null)
        {
            // Get the error classification
            var classification = ex.GetErrorClassification();

            // Get the error severity
            var severity = DetermineSeverity(classification);

            // Create the error context
            var errorContext = new ErrorContext
            {
                ErrorCode = ex is FlowOrchestratorException flowEx ? flowEx.Classification.ToString() : classification.ToString(),
                ErrorMessage = ex.Message,
                Classification = classification,
                Severity = severity,
                ServiceId = serviceId,
                ExecutionId = executionId,
                BranchId = branchId,
                StepId = stepId,
                Timestamp = DateTime.UtcNow,
                Exception = ex
            };

            // Add error details
            if (ex is FlowOrchestratorException flowOrchestratorException)
            {
                foreach (var detail in flowOrchestratorException.Details)
                {
                    errorContext.Details[detail.Key] = detail.Value;
                }
            }

            return errorContext;
        }

        private ErrorSeverity DetermineSeverity(ErrorClassification classification)
        {
            return classification switch
            {
                ErrorClassification.CONNECTION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_TIMEOUT => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_UNREACHABLE => ErrorSeverity.MAJOR,
                ErrorClassification.CONNECTION_HANDSHAKE_FAILURE => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_INVALID_CREDENTIALS => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_EXPIRED_TOKEN => ErrorSeverity.MAJOR,
                ErrorClassification.AUTHENTICATION_INSUFFICIENT_PERMISSIONS => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_INVALID_FORMAT => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_SCHEMA_VIOLATION => ErrorSeverity.MAJOR,
                ErrorClassification.DATA_CORRUPTION => ErrorSeverity.CRITICAL,
                ErrorClassification.RESOURCE_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_NOT_FOUND => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_UNAVAILABLE => ErrorSeverity.MAJOR,
                ErrorClassification.RESOURCE_QUOTA_EXCEEDED => ErrorSeverity.MAJOR,
                ErrorClassification.PROCESSING_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.VALIDATION_ERROR => ErrorSeverity.MINOR,
                ErrorClassification.CONFIGURATION_ERROR => ErrorSeverity.MAJOR,
                ErrorClassification.VERSION_COMPATIBILITY_ERROR => ErrorSeverity.CRITICAL,
                ErrorClassification.GENERAL_ERROR => ErrorSeverity.MAJOR,
                _ => ErrorSeverity.MAJOR
            };
        }

        private void CleanupOldRecoveries()
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-_options.RecoveryHistoryRetentionHours);
            var oldRecoveries = _activeRecoveries
                .Where(kvp => kvp.Value.StartTimestamp < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var recoveryId in oldRecoveries)
            {
                _activeRecoveries.TryRemove(recoveryId, out _);
            }

            if (oldRecoveries.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old recovery contexts", oldRecoveries.Count);
            }
        }
    }

    /// <summary>
    /// Options for the recovery service.
    /// </summary>
    public class RecoveryServiceOptions
    {
        /// <summary>
        /// Gets or sets the number of hours to retain recovery history.
        /// </summary>
        public int RecoveryHistoryRetentionHours { get; set; } = 24;

        /// <summary>
        /// Gets or sets the maximum number of active recoveries.
        /// </summary>
        public int MaxActiveRecoveries { get; set; } = 1000;
    }
}
