using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Monitoring.HealthChecks
{
    /// <summary>
    /// Health check for resource utilization.
    /// </summary>
    public class ResourceUtilizationHealthCheck : IHealthCheck
    {
        private readonly ILogger<ResourceUtilizationHealthCheck> _logger;
        private readonly IResourceUtilizationMonitorService _resourceUtilizationMonitorService;
        private readonly IOptions<MonitoringOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceUtilizationHealthCheck"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="resourceUtilizationMonitorService">The resource utilization monitor service.</param>
        /// <param name="options">The monitoring options.</param>
        public ResourceUtilizationHealthCheck(
            ILogger<ResourceUtilizationHealthCheck> logger,
            IResourceUtilizationMonitorService resourceUtilizationMonitorService,
            IOptions<MonitoringOptions> options)
        {
            _logger = logger;
            _resourceUtilizationMonitorService = resourceUtilizationMonitorService;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var utilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                var data = new Dictionary<string, object>
                {
                    ["CpuUsage"] = utilization.Cpu.TotalUsagePercent,
                    ["MemoryUsage"] = utilization.Memory.UsagePercent,
                    ["ProcessorCount"] = utilization.Cpu.ProcessorCount,
                    ["TotalMemory"] = utilization.Memory.TotalPhysicalMemory,
                    ["AvailableMemory"] = utilization.Memory.AvailablePhysicalMemory
                };

                if (utilization.Cpu.TotalUsagePercent > _options.Value.CpuUsageThresholdPercent)
                {
                    return HealthCheckResult.Degraded(
                        $"CPU usage ({utilization.Cpu.TotalUsagePercent:F1}%) exceeds threshold ({_options.Value.CpuUsageThresholdPercent}%)",
                        data: data);
                }

                if (utilization.Memory.UsagePercent > _options.Value.MemoryUsageThresholdPercent)
                {
                    return HealthCheckResult.Degraded(
                        $"Memory usage ({utilization.Memory.UsagePercent:F1}%) exceeds threshold ({_options.Value.MemoryUsageThresholdPercent}%)",
                        data: data);
                }

                foreach (var disk in utilization.Disks)
                {
                    if (disk.UsagePercent > _options.Value.DiskUsageThresholdPercent)
                    {
                        data["DiskName"] = disk.DriveName;
                        data["DiskUsage"] = disk.UsagePercent;
                        
                        return HealthCheckResult.Degraded(
                            $"Disk usage for {disk.DriveName} ({disk.UsagePercent:F1}%) exceeds threshold ({_options.Value.DiskUsageThresholdPercent}%)",
                            data: data);
                    }
                }

                return HealthCheckResult.Healthy("Resource utilization is within normal limits", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking resource utilization health");
                return HealthCheckResult.Unhealthy("Error checking resource utilization health", ex);
            }
        }
    }
}
