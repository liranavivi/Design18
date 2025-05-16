using FlowOrchestrator.Observability.Monitoring.Models;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Interface for the resource utilization monitor service.
    /// </summary>
    public interface IResourceUtilizationMonitorService
    {
        /// <summary>
        /// Gets the current resource utilization.
        /// </summary>
        /// <returns>The resource utilization response.</returns>
        Task<ResourceUtilizationResponse> GetResourceUtilizationAsync();

        /// <summary>
        /// Gets the resource utilization history.
        /// </summary>
        /// <param name="timeSpan">The time span to retrieve history for.</param>
        /// <param name="intervalSeconds">The interval between data points in seconds.</param>
        /// <returns>The list of resource utilization responses.</returns>
        Task<List<ResourceUtilizationResponse>> GetResourceUtilizationHistoryAsync(TimeSpan timeSpan, int intervalSeconds = 60);

        /// <summary>
        /// Gets the resource utilization for a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service resource utilization.</returns>
        Task<ServiceResourceUtilization> GetServiceResourceUtilizationAsync(string serviceId);

        /// <summary>
        /// Checks if resource utilization exceeds thresholds.
        /// </summary>
        /// <returns>True if any resource utilization exceeds thresholds; otherwise, false.</returns>
        Task<bool> CheckResourceThresholdsAsync();

        /// <summary>
        /// Gets the resource utilization thresholds.
        /// </summary>
        /// <returns>The dictionary of resource thresholds.</returns>
        Dictionary<string, double> GetResourceThresholds();

        /// <summary>
        /// Sets a resource utilization threshold.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <param name="thresholdPercent">The threshold percentage.</param>
        void SetResourceThreshold(string resourceName, double thresholdPercent);
    }
}
