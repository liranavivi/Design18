namespace FlowOrchestrator.Observability.Monitoring.Models
{
    /// <summary>
    /// Configuration options for the monitoring framework.
    /// </summary>
    public class MonitoringOptions
    {
        /// <summary>
        /// Gets or sets the service ID.
        /// </summary>
        public string ServiceId { get; set; } = "FlowOrchestrator.MonitoringFramework";

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the health check interval in seconds.
        /// </summary>
        public int HealthCheckIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the resource monitoring interval in seconds.
        /// </summary>
        public int ResourceMonitoringIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Gets or sets the flow monitoring interval in seconds.
        /// </summary>
        public int FlowMonitoringIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the performance anomaly detection interval in seconds.
        /// </summary>
        public int AnomalyDetectionIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the alert check interval in seconds.
        /// </summary>
        public int AlertCheckIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the data retention period in days.
        /// </summary>
        public int DataRetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the CPU usage threshold percentage for alerts.
        /// </summary>
        public double CpuUsageThresholdPercent { get; set; } = 80.0;

        /// <summary>
        /// Gets or sets the memory usage threshold percentage for alerts.
        /// </summary>
        public double MemoryUsageThresholdPercent { get; set; } = 80.0;

        /// <summary>
        /// Gets or sets the disk usage threshold percentage for alerts.
        /// </summary>
        public double DiskUsageThresholdPercent { get; set; } = 85.0;

        /// <summary>
        /// Gets or sets the service discovery endpoints.
        /// </summary>
        public List<string> ServiceDiscoveryEndpoints { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to enable automatic service discovery.
        /// </summary>
        public bool EnableAutoDiscovery { get; set; } = true;
    }
}
