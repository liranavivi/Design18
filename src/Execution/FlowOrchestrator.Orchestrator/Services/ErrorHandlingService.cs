using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static FlowOrchestrator.Orchestrator.Models.ExecutionErrorExtensions;

namespace FlowOrchestrator.Orchestrator.Services
{
    /// <summary>
    /// Service for handling errors and implementing recovery strategies.
    /// </summary>
    public class ErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlingService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBus">The message bus.</param>
        public ErrorHandlingService(ILogger<ErrorHandlingService> logger, IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _circuitBreakers = new ConcurrentDictionary<string, CircuitBreakerState>();
        }

        /// <summary>
        /// Handles an error in a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="error">The error information.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The recovery action to take.</returns>
        public RecoveryAction HandleError(string executionId, string branchId, ExecutionError error, FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            _logger.LogError("Handling error in execution {ExecutionId}, branch {BranchId}: {ErrorMessage}",
                executionId, branchId, ExecutionErrorExtensions.Message(error));

            // Determine the error type and severity
            var errorType = DetermineErrorType(error);
            var severity = DetermineSeverity(error, errorType);

            // Check if the circuit breaker is open for this service
            var serviceId = error.ServiceId ?? "unknown";
            var circuitBreakerKey = $"{serviceId}:{error.ErrorCode}";

            if (_circuitBreakers.TryGetValue(circuitBreakerKey, out var circuitBreaker) &&
                circuitBreaker.State == CircuitState.OPEN)
            {
                _logger.LogWarning("Circuit breaker is open for service {ServiceId}, error code {ErrorCode}",
                    serviceId, error.ErrorCode);
                return RecoveryAction.FAIL_FAST;
            }

            // Determine the recovery action based on error type and severity
            var recoveryAction = DetermineRecoveryAction(errorType, severity, error);

            // Update circuit breaker state if needed
            if (recoveryAction == RecoveryAction.FAIL_FAST)
            {
                UpdateCircuitBreaker(circuitBreakerKey, CircuitState.OPEN);
            }
            else if (recoveryAction == RecoveryAction.RETRY)
            {
                // Increment failure count in circuit breaker
                if (_circuitBreakers.TryGetValue(circuitBreakerKey, out circuitBreaker))
                {
                    circuitBreaker.FailureCount++;
                    if (circuitBreaker.FailureCount >= circuitBreaker.FailureThreshold)
                    {
                        _logger.LogWarning("Circuit breaker threshold reached for service {ServiceId}, error code {ErrorCode}",
                            serviceId, error.ErrorCode);
                        UpdateCircuitBreaker(circuitBreakerKey, CircuitState.OPEN);
                        return RecoveryAction.FAIL_FAST;
                    }
                }
                else
                {
                    // Create new circuit breaker
                    _circuitBreakers[circuitBreakerKey] = new CircuitBreakerState
                    {
                        State = CircuitState.CLOSED,
                        FailureCount = 1,
                        FailureThreshold = 3,
                        LastFailureTime = DateTime.UtcNow
                    };
                }
            }

            _logger.LogInformation("Recovery action for error in execution {ExecutionId}, branch {BranchId}: {RecoveryAction}",
                executionId, branchId, recoveryAction);

            return recoveryAction;
        }

        /// <summary>
        /// Determines the type of error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <returns>The error type.</returns>
        private ErrorType DetermineErrorType(ExecutionError error)
        {
            // In a real implementation, this would analyze the error details
            // to determine the type of error (e.g., transient, permanent, etc.)

            // For now, we'll use a simple heuristic based on the error code
            if (error.ErrorCode?.StartsWith("TRANSIENT_") == true)
            {
                return ErrorType.TRANSIENT;
            }
            else if (error.ErrorCode?.StartsWith("CONFIG_") == true)
            {
                return ErrorType.CONFIGURATION;
            }
            else if (error.ErrorCode?.StartsWith("DATA_") == true)
            {
                return ErrorType.DATA;
            }
            else if (error.ErrorCode?.StartsWith("SYSTEM_") == true)
            {
                return ErrorType.SYSTEM;
            }
            else
            {
                return ErrorType.UNKNOWN;
            }
        }

        /// <summary>
        /// Determines the severity of an error.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="errorType">The error type.</param>
        /// <returns>The error severity.</returns>
        private ErrorSeverity DetermineSeverity(ExecutionError error, ErrorType errorType)
        {
            // In a real implementation, this would analyze the error details
            // to determine the severity of the error

            // For now, we'll use a simple mapping from error type to severity
            switch (errorType)
            {
                case ErrorType.TRANSIENT:
                    return ErrorSeverity.LOW;
                case ErrorType.CONFIGURATION:
                    return ErrorSeverity.MEDIUM;
                case ErrorType.DATA:
                    return ErrorSeverity.MEDIUM;
                case ErrorType.SYSTEM:
                    return ErrorSeverity.HIGH;
                case ErrorType.UNKNOWN:
                default:
                    return ErrorSeverity.MEDIUM;
            }
        }

        /// <summary>
        /// Determines the recovery action for an error.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="severity">The error severity.</param>
        /// <param name="error">The error information.</param>
        /// <returns>The recovery action.</returns>
        private RecoveryAction DetermineRecoveryAction(ErrorType errorType, ErrorSeverity severity, ExecutionError error)
        {
            // In a real implementation, this would use a more sophisticated
            // decision tree based on error type, severity, and other factors

            // For now, we'll use a simple mapping
            switch (errorType)
            {
                case ErrorType.TRANSIENT:
                    return RecoveryAction.RETRY;
                case ErrorType.CONFIGURATION:
                    return RecoveryAction.FAIL_BRANCH;
                case ErrorType.DATA:
                    return severity == ErrorSeverity.HIGH ? RecoveryAction.FAIL_EXECUTION : RecoveryAction.FAIL_BRANCH;
                case ErrorType.SYSTEM:
                    return RecoveryAction.FAIL_EXECUTION;
                case ErrorType.UNKNOWN:
                default:
                    return RecoveryAction.FAIL_BRANCH;
            }
        }

        /// <summary>
        /// Updates the state of a circuit breaker.
        /// </summary>
        /// <param name="key">The circuit breaker key.</param>
        /// <param name="state">The new state.</param>
        private void UpdateCircuitBreaker(string key, CircuitState state)
        {
            if (_circuitBreakers.TryGetValue(key, out var circuitBreaker))
            {
                circuitBreaker.State = state;
                circuitBreaker.LastStateChangeTime = DateTime.UtcNow;

                if (state == CircuitState.OPEN)
                {
                    // Schedule a task to reset the circuit breaker after a timeout
                    Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ =>
                    {
                        if (_circuitBreakers.TryGetValue(key, out var cb) && cb.State == CircuitState.OPEN)
                        {
                            _logger.LogInformation("Resetting circuit breaker for key {Key} to half-open state", key);
                            cb.State = CircuitState.HALF_OPEN;
                            cb.FailureCount = 0;
                            cb.LastStateChangeTime = DateTime.UtcNow;
                        }
                    });
                }
            }
            else
            {
                _circuitBreakers[key] = new CircuitBreakerState
                {
                    State = state,
                    FailureCount = state == CircuitState.OPEN ? 1 : 0,
                    FailureThreshold = 3,
                    LastFailureTime = DateTime.UtcNow,
                    LastStateChangeTime = DateTime.UtcNow
                };
            }
        }
    }

    /// <summary>
    /// Represents the state of a circuit breaker.
    /// </summary>
    public class CircuitBreakerState
    {
        /// <summary>
        /// Gets or sets the circuit state.
        /// </summary>
        public CircuitState State { get; set; } = CircuitState.CLOSED;

        /// <summary>
        /// Gets or sets the failure count.
        /// </summary>
        public int FailureCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the failure threshold.
        /// </summary>
        public int FailureThreshold { get; set; } = 3;

        /// <summary>
        /// Gets or sets the time of the last failure.
        /// </summary>
        public DateTime LastFailureTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the time of the last state change.
        /// </summary>
        public DateTime LastStateChangeTime { get; set; } = DateTime.MinValue;
    }

    /// <summary>
    /// Represents the state of a circuit.
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// The circuit is closed and allowing requests.
        /// </summary>
        CLOSED,

        /// <summary>
        /// The circuit is open and blocking requests.
        /// </summary>
        OPEN,

        /// <summary>
        /// The circuit is half-open and allowing a limited number of requests.
        /// </summary>
        HALF_OPEN
    }

    /// <summary>
    /// Represents the type of error.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// A transient error that may resolve itself.
        /// </summary>
        TRANSIENT,

        /// <summary>
        /// A configuration error.
        /// </summary>
        CONFIGURATION,

        /// <summary>
        /// A data error.
        /// </summary>
        DATA,

        /// <summary>
        /// A system error.
        /// </summary>
        SYSTEM,

        /// <summary>
        /// An unknown error.
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// Represents the severity of an error.
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// A low severity error.
        /// </summary>
        LOW,

        /// <summary>
        /// A medium severity error.
        /// </summary>
        MEDIUM,

        /// <summary>
        /// A high severity error.
        /// </summary>
        HIGH
    }

    /// <summary>
    /// Represents the recovery action to take for an error.
    /// </summary>
    public enum RecoveryAction
    {
        /// <summary>
        /// Retry the operation.
        /// </summary>
        RETRY,

        /// <summary>
        /// Fail the branch but continue with other branches.
        /// </summary>
        FAIL_BRANCH,

        /// <summary>
        /// Fail the entire execution.
        /// </summary>
        FAIL_EXECUTION,

        /// <summary>
        /// Fail fast without retrying.
        /// </summary>
        FAIL_FAST
    }
}
