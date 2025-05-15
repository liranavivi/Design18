namespace FlowOrchestrator.Abstractions.Statistics
{
    /// <summary>
    /// Defines the interface for statistics providers in the FlowOrchestrator system.
    /// </summary>
    public interface IStatisticsProvider
    {
        /// <summary>
        /// Gets the provider identifier.
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Gets the provider type.
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <returns>The statistics.</returns>
        Dictionary<string, object> GetStatistics();

        /// <summary>
        /// Gets a specific statistic.
        /// </summary>
        /// <param name="key">The statistic key.</param>
        /// <returns>The statistic value.</returns>
        object GetStatistic(string key);

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        void ResetStatistics();

        /// <summary>
        /// Starts tracking an operation.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        void StartOperation(string operationName);

        /// <summary>
        /// Ends tracking an operation.
        /// </summary>
        /// <param name="operationName">The operation name.</param>
        /// <param name="result">The operation result.</param>
        void EndOperation(string operationName, OperationResult result);

        /// <summary>
        /// Records a metric.
        /// </summary>
        /// <param name="metricName">The metric name.</param>
        /// <param name="value">The metric value.</param>
        void RecordMetric(string metricName, object value);

        /// <summary>
        /// Increments a counter.
        /// </summary>
        /// <param name="counterName">The counter name.</param>
        /// <param name="increment">The increment value.</param>
        void IncrementCounter(string counterName, int increment = 1);

        /// <summary>
        /// Records a duration.
        /// </summary>
        /// <param name="durationName">The duration name.</param>
        /// <param name="milliseconds">The duration in milliseconds.</param>
        void RecordDuration(string durationName, long milliseconds);

        /// <summary>
        /// Gets the operation statistics.
        /// </summary>
        /// <returns>The operation statistics.</returns>
        Dictionary<string, object> GetOperationStatistics();
    }

    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public enum OperationResult
    {
        /// <summary>
        /// Operation succeeded.
        /// </summary>
        SUCCESS,

        /// <summary>
        /// Operation failed.
        /// </summary>
        FAILURE,

        /// <summary>
        /// Operation was cancelled.
        /// </summary>
        CANCELLED,

        /// <summary>
        /// Operation timed out.
        /// </summary>
        TIMEOUT
    }
}
