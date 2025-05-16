using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Interface for the health check service.
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Gets the health status of all services.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The system status response.</returns>
        Task<SystemStatusResponse> GetSystemHealthStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the health status of a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service health status.</returns>
        Task<ServiceHealthStatus> GetServiceHealthStatusAsync(string serviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a health check on all services.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The health report.</returns>
        Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Discovers available services.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of service health statuses.</returns>
        Task<List<ServiceHealthStatus>> DiscoverServicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a service for health monitoring.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="endpoint">The service endpoint.</param>
        /// <returns>True if the service was registered successfully; otherwise, false.</returns>
        bool RegisterService(string serviceId, string serviceType, string endpoint);

        /// <summary>
        /// Unregisters a service from health monitoring.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>True if the service was unregistered successfully; otherwise, false.</returns>
        bool UnregisterService(string serviceId);
    }
}
