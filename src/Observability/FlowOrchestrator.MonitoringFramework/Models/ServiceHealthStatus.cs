using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Health status information for a service.
    /// </summary>
    public class ServiceHealthStatus
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public string ServiceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service state.
        /// </summary>
        public ServiceState State { get; set; }

        /// <summary>
        /// Gets or sets the service health status.
        /// </summary>
        public string HealthStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service health details.
        /// </summary>
        public string HealthDetails { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service endpoint.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last check timestamp.
        /// </summary>
        public DateTime LastCheckTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the service uptime.
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// Gets or sets the resource utilization.
        /// </summary>
        public ServiceResourceUtilization ResourceUtilization { get; set; } = new ServiceResourceUtilization();
    }

    /// <summary>
    /// Resource utilization information for a service.
    /// </summary>
    public class ServiceResourceUtilization
    {
        /// <summary>
        /// Gets or sets the CPU usage percentage.
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the memory usage in bytes.
        /// </summary>
        public long MemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the memory usage percentage.
        /// </summary>
        public double MemoryUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the thread count.
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the handle count.
        /// </summary>
        public int HandleCount { get; set; }
    }

    /// <summary>
    /// System status response containing health information for all services.
    /// </summary>
    public class SystemStatusResponse
    {
        /// <summary>
        /// Gets or sets the overall system status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service health statuses.
        /// </summary>
        public List<ServiceHealthStatus> Services { get; set; } = new List<ServiceHealthStatus>();

        /// <summary>
        /// Gets or sets the resource utilization.
        /// </summary>
        public ResourceUtilizationResponse ResourceUtilization { get; set; } = new ResourceUtilizationResponse();

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
