using FlowOrchestrator.Observability.Monitoring.Models;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Interface for the performance anomaly detector service.
    /// </summary>
    public interface IPerformanceAnomalyDetectorService
    {
        /// <summary>
        /// Detects anomalies in resource utilization.
        /// </summary>
        /// <returns>The list of alerts for detected anomalies.</returns>
        Task<List<AlertModel>> DetectResourceAnomaliesAsync();

        /// <summary>
        /// Detects anomalies in flow execution.
        /// </summary>
        /// <returns>The list of alerts for detected anomalies.</returns>
        Task<List<AlertModel>> DetectFlowExecutionAnomaliesAsync();

        /// <summary>
        /// Gets performance anomalies for a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The list of alerts for detected anomalies.</returns>
        Task<List<AlertModel>> GetServiceAnomaliesAsync(string serviceId);

        /// <summary>
        /// Gets performance anomalies for a specific flow.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The list of alerts for detected anomalies.</returns>
        Task<List<AlertModel>> GetFlowAnomaliesAsync(string flowId);

        /// <summary>
        /// Gets all detected anomalies.
        /// </summary>
        /// <returns>The list of alerts for all detected anomalies.</returns>
        Task<List<AlertModel>> GetAllAnomaliesAsync();

        /// <summary>
        /// Configures anomaly detection thresholds.
        /// </summary>
        /// <param name="metricName">The metric name.</param>
        /// <param name="thresholdValue">The threshold value.</param>
        /// <param name="sensitivityLevel">The sensitivity level.</param>
        void ConfigureAnomalyDetection(string metricName, double thresholdValue, int sensitivityLevel = 3);
    }
}
