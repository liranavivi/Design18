using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Recovery.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlowOrchestrator.Recovery.Strategies
{
    /// <summary>
    /// A recovery strategy that implements the circuit breaker pattern.
    /// </summary>
    public class CircuitBreakerRecoveryStrategy : AbstractRecoveryStrategy
    {
        private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerRecoveryStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CircuitBreakerRecoveryStrategy(ILogger<CircuitBreakerRecoveryStrategy> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the name of the strategy.
        /// </summary>
        public override string Name => "CircuitBreakerStrategy";

        /// <summary>
        /// Gets the description of the strategy.
        /// </summary>
        public override string Description => "Prevents cascading failures by failing fast when a service is experiencing problems";

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(Dictionary<string, object> parameters)
        {
            var errors = new List<string>();

            // Validate FailureThreshold
            if (parameters.TryGetValue("FailureThreshold", out var failureThresholdObj))
            {
                if (failureThresholdObj is not int failureThreshold || failureThreshold <= 0)
                {
                    errors.Add("FailureThreshold must be a positive integer");
                }
            }

            // Validate ResetTimeoutSeconds
            if (parameters.TryGetValue("ResetTimeoutSeconds", out var resetTimeoutObj))
            {
                if (resetTimeoutObj is not int resetTimeout || resetTimeout <= 0)
                {
                    errors.Add("ResetTimeoutSeconds must be a positive integer");
                }
            }

            // Validate SuccessThreshold
            if (parameters.TryGetValue("SuccessThreshold", out var successThresholdObj))
            {
                if (successThresholdObj is not int successThreshold || successThreshold <= 0)
                {
                    errors.Add("SuccessThreshold must be a positive integer");
                }
            }

            return errors.Count > 0 
                ? ValidationResult.Invalid(errors) 
                : ValidationResult.Valid();
        }

        /// <summary>
        /// Determines whether this strategy is applicable to the specified error context.
        /// </summary>
        /// <param name="errorContext">The error context.</param>
        /// <returns><c>true</c> if this strategy is applicable; otherwise, <c>false</c>.</returns>
        public override bool IsApplicable(ErrorContext errorContext)
        {
            // Circuit breaker is applicable to most error types
            return true;
        }

        /// <summary>
        /// Executes the recovery strategy.
        /// </summary>
        /// <param name="context">The recovery context.</param>
        /// <returns>The recovery result.</returns>
        public override RecoveryResult Recover(RecoveryContext context)
        {
            _logger.LogInformation("Executing circuit breaker recovery strategy for error {ErrorCode}", 
                context.ErrorContext.ErrorCode);

            // Get configuration values
            int failureThreshold = GetConfigValue("FailureThreshold", 3);
            int resetTimeoutSeconds = GetConfigValue("ResetTimeoutSeconds", 60);
            int successThreshold = GetConfigValue("SuccessThreshold", 2);

            // Create a circuit ID based on service ID and error code
            string serviceId = context.ErrorContext.ServiceId;
            string errorCode = context.ErrorContext.ErrorCode;
            string circuitId = $"{serviceId}:{errorCode}";

            // Get or create the circuit breaker state
            var circuitBreaker = _circuitBreakers.GetOrAdd(circuitId, _ => new CircuitBreakerState
            {
                CircuitId = circuitId,
                ServiceId = serviceId,
                ErrorCode = errorCode,
                FailureThreshold = failureThreshold,
                ResetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds),
                SuccessThreshold = successThreshold
            });

            // Check if the circuit should be reset
            if (circuitBreaker.ShouldReset)
            {
                _logger.LogInformation("Resetting circuit breaker {CircuitId} to half-open state", circuitId);
                circuitBreaker.State = CircuitState.HALF_OPEN;
                circuitBreaker.FailureCount = 0;
                circuitBreaker.SuccessCount = 0;
                circuitBreaker.LastStateChangeTime = DateTime.UtcNow;
            }

            // Handle the error based on the current circuit state
            switch (circuitBreaker.State)
            {
                case CircuitState.CLOSED:
                    return HandleClosedState(context, circuitBreaker);
                
                case CircuitState.OPEN:
                    return HandleOpenState(context, circuitBreaker);
                
                case CircuitState.HALF_OPEN:
                    return HandleHalfOpenState(context, circuitBreaker);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RecoveryResult HandleClosedState(RecoveryContext context, CircuitBreakerState circuitBreaker)
        {
            // Increment the failure count
            circuitBreaker.FailureCount++;
            circuitBreaker.LastFailureTime = DateTime.UtcNow;

            _logger.LogDebug("Circuit {CircuitId} failure count: {FailureCount}/{FailureThreshold}", 
                circuitBreaker.CircuitId, circuitBreaker.FailureCount, circuitBreaker.FailureThreshold);

            // Check if the failure threshold has been reached
            if (circuitBreaker.FailureCount >= circuitBreaker.FailureThreshold)
            {
                _logger.LogWarning("Circuit {CircuitId} is now OPEN (failure threshold reached)", 
                    circuitBreaker.CircuitId);
                
                // Open the circuit
                circuitBreaker.State = CircuitState.OPEN;
                circuitBreaker.LastStateChangeTime = DateTime.UtcNow;

                // Return a fail-fast result
                return RecoveryResult.Failed(
                    RecoveryAction.FAIL_FAST,
                    $"Circuit breaker {circuitBreaker.CircuitId} is now open",
                    context,
                    context.ErrorContext);
            }

            // The circuit is still closed, so we can retry
            return RecoveryResult.Successful(
                RecoveryAction.RETRY,
                $"Circuit breaker {circuitBreaker.CircuitId} is closed, allowing retry",
                context);
        }

        private RecoveryResult HandleOpenState(RecoveryContext context, CircuitBreakerState circuitBreaker)
        {
            _logger.LogWarning("Circuit {CircuitId} is OPEN, failing fast", circuitBreaker.CircuitId);

            // The circuit is open, so we fail fast
            return RecoveryResult.Failed(
                RecoveryAction.FAIL_FAST,
                $"Circuit breaker {circuitBreaker.CircuitId} is open, failing fast",
                context,
                context.ErrorContext);
        }

        private RecoveryResult HandleHalfOpenState(RecoveryContext context, CircuitBreakerState circuitBreaker)
        {
            // In half-open state, we allow a limited number of requests through
            // Since this is an error, increment the failure count
            circuitBreaker.FailureCount++;
            circuitBreaker.LastFailureTime = DateTime.UtcNow;

            _logger.LogWarning("Circuit {CircuitId} is HALF-OPEN and experienced a failure", 
                circuitBreaker.CircuitId);

            // If we get a failure in half-open state, we go back to open
            circuitBreaker.State = CircuitState.OPEN;
            circuitBreaker.LastStateChangeTime = DateTime.UtcNow;

            return RecoveryResult.Failed(
                RecoveryAction.FAIL_FAST,
                $"Circuit breaker {circuitBreaker.CircuitId} is now open again after failure in half-open state",
                context,
                context.ErrorContext);
        }

        /// <summary>
        /// Records a successful operation for a circuit.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="errorCode">The error code.</param>
        public void RecordSuccess(string serviceId, string errorCode)
        {
            string circuitId = $"{serviceId}:{errorCode}";

            if (_circuitBreakers.TryGetValue(circuitId, out var circuitBreaker))
            {
                if (circuitBreaker.State == CircuitState.HALF_OPEN)
                {
                    // Increment the success count
                    circuitBreaker.SuccessCount++;

                    // Check if we've reached the success threshold
                    if (circuitBreaker.SuccessCount >= circuitBreaker.SuccessThreshold)
                    {
                        _logger.LogInformation("Circuit {CircuitId} is now CLOSED (success threshold reached)", 
                            circuitId);
                        
                        // Close the circuit
                        circuitBreaker.State = CircuitState.CLOSED;
                        circuitBreaker.FailureCount = 0;
                        circuitBreaker.SuccessCount = 0;
                        circuitBreaker.LastStateChangeTime = DateTime.UtcNow;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the state of a circuit.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The circuit state.</returns>
        public CircuitState GetCircuitState(string serviceId, string errorCode)
        {
            string circuitId = $"{serviceId}:{errorCode}";

            if (_circuitBreakers.TryGetValue(circuitId, out var circuitBreaker))
            {
                // Check if the circuit should be reset
                if (circuitBreaker.ShouldReset)
                {
                    _logger.LogInformation("Resetting circuit breaker {CircuitId} to half-open state", circuitId);
                    circuitBreaker.State = CircuitState.HALF_OPEN;
                    circuitBreaker.FailureCount = 0;
                    circuitBreaker.SuccessCount = 0;
                    circuitBreaker.LastStateChangeTime = DateTime.UtcNow;
                }

                return circuitBreaker.State;
            }

            // If the circuit doesn't exist, it's considered closed
            return CircuitState.CLOSED;
        }
    }
}
