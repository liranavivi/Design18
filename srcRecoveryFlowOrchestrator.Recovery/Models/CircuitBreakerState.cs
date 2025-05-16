namespace FlowOrchestrator.Recovery.Models
{
    /// <summary>
    /// Represents the state of a circuit breaker.
    /// </summary>
    public class CircuitBreakerState
    {
        /// <summary>
        /// Gets or sets the circuit identifier.
        /// </summary>
        public string CircuitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the circuit state.
        /// </summary>
        public CircuitState State { get; set; } = CircuitState.CLOSED;

        /// <summary>
        /// Gets or sets the failure count.
        /// </summary>
        public int FailureCount { get; set; }

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

        /// <summary>
        /// Gets or sets the reset timeout.
        /// </summary>
        public TimeSpan ResetTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets the success count in half-open state.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the success threshold to reset the circuit.
        /// </summary>
        public int SuccessThreshold { get; set; } = 2;

        /// <summary>
        /// Gets a value indicating whether the circuit is open.
        /// </summary>
        public bool IsOpen => State == CircuitState.OPEN;

        /// <summary>
        /// Gets a value indicating whether the circuit is closed.
        /// </summary>
        public bool IsClosed => State == CircuitState.CLOSED;

        /// <summary>
        /// Gets a value indicating whether the circuit is half-open.
        /// </summary>
        public bool IsHalfOpen => State == CircuitState.HALF_OPEN;

        /// <summary>
        /// Gets a value indicating whether the circuit should be reset based on the reset timeout.
        /// </summary>
        public bool ShouldReset => State == CircuitState.OPEN && 
                                  (DateTime.UtcNow - LastStateChangeTime) > ResetTimeout;
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
}
