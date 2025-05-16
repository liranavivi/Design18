using FlowOrchestrator.Observability.Monitoring.Models;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Observability.Monitoring.Services
{
    /// <summary>
    /// Implementation of the performance anomaly detector service.
    /// </summary>
    public class PerformanceAnomalyDetectorService : IPerformanceAnomalyDetectorService
    {
        private readonly ILogger<PerformanceAnomalyDetectorService> _logger;
        private readonly IOptions<MonitoringOptions> _options;
        private readonly IResourceUtilizationMonitorService _resourceUtilizationMonitorService;
        private readonly IActiveFlowMonitorService _activeFlowMonitorService;
        private readonly IAlertManagerService _alertManagerService;
        private readonly Dictionary<string, double> _thresholds = new();
        private readonly Dictionary<string, int> _sensitivityLevels = new();
        private readonly Dictionary<string, List<double>> _metricHistory = new();
        private readonly List<AlertModel> _detectedAnomalies = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnomalyDetectorService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The monitoring options.</param>
        /// <param name="resourceUtilizationMonitorService">The resource utilization monitor service.</param>
        /// <param name="activeFlowMonitorService">The active flow monitor service.</param>
        /// <param name="alertManagerService">The alert manager service.</param>
        public PerformanceAnomalyDetectorService(
            ILogger<PerformanceAnomalyDetectorService> logger,
            IOptions<MonitoringOptions> options,
            IResourceUtilizationMonitorService resourceUtilizationMonitorService,
            IActiveFlowMonitorService activeFlowMonitorService,
            IAlertManagerService alertManagerService)
        {
            _logger = logger;
            _options = options;
            _resourceUtilizationMonitorService = resourceUtilizationMonitorService;
            _activeFlowMonitorService = activeFlowMonitorService;
            _alertManagerService = alertManagerService;

            // Initialize default thresholds
            _thresholds["CPU"] = options.Value.CpuUsageThresholdPercent;
            _thresholds["Memory"] = options.Value.MemoryUsageThresholdPercent;
            _thresholds["Disk"] = options.Value.DiskUsageThresholdPercent;
            _thresholds["FlowExecutionTime"] = 60000; // 60 seconds

            // Initialize default sensitivity levels
            _sensitivityLevels["CPU"] = 3;
            _sensitivityLevels["Memory"] = 3;
            _sensitivityLevels["Disk"] = 3;
            _sensitivityLevels["FlowExecutionTime"] = 3;
        }

        /// <inheritdoc/>
        public void ConfigureAnomalyDetection(string metricName, double thresholdValue, int sensitivityLevel = 3)
        {
            _thresholds[metricName] = thresholdValue;
            _sensitivityLevels[metricName] = sensitivityLevel;
            _logger.LogInformation("Configured anomaly detection for {MetricName}: threshold={Threshold}, sensitivity={Sensitivity}",
                metricName, thresholdValue, sensitivityLevel);
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> DetectFlowExecutionAnomaliesAsync()
        {
            var anomalies = new List<AlertModel>();

            try
            {
                var activeFlows = await _activeFlowMonitorService.GetActiveFlowsAsync();
                
                foreach (var flow in activeFlows)
                {
                    // Check for long-running flows
                    if (flow.ExecutionDuration.TotalMilliseconds > _thresholds["FlowExecutionTime"])
                    {
                        var alert = new AlertModel
                        {
                            Name = "Long Running Flow",
                            Description = $"Flow {flow.FlowName} (ID: {flow.FlowId}) has been running for {flow.ExecutionDuration.TotalSeconds:F1} seconds",
                            Severity = AlertSeverity.Warning,
                            Source = "FlowMonitor",
                            Category = "Performance",
                            Details = new Dictionary<string, object>
                            {
                                ["FlowId"] = flow.FlowId,
                                ["FlowName"] = flow.FlowName,
                                ["ExecutionId"] = flow.ExecutionId,
                                ["ExecutionDuration"] = flow.ExecutionDuration.TotalMilliseconds,
                                ["CurrentStep"] = flow.CurrentStep
                            }
                        };
                        
                        anomalies.Add(alert);
                        _detectedAnomalies.Add(alert);
                        
                        await _alertManagerService.CreateAlertAsync(alert);
                    }
                    
                    // Check for flows with errors
                    if (flow.Error != null)
                    {
                        var alert = new AlertModel
                        {
                            Name = "Flow Execution Error",
                            Description = $"Flow {flow.FlowName} (ID: {flow.FlowId}) encountered an error: {flow.Error.Message}",
                            Severity = AlertSeverity.Error,
                            Source = "FlowMonitor",
                            Category = "Error",
                            Details = new Dictionary<string, object>
                            {
                                ["FlowId"] = flow.FlowId,
                                ["FlowName"] = flow.FlowName,
                                ["ExecutionId"] = flow.ExecutionId,
                                ["ErrorMessage"] = flow.Error.Message,
                                ["ErrorType"] = flow.Error.ErrorType,
                                ["ErrorTimestamp"] = flow.Error.Timestamp,
                                ["ErrorStep"] = flow.Error.Step,
                                ["RecoveryAttempts"] = flow.Error.RecoveryAttempts
                            }
                        };
                        
                        anomalies.Add(alert);
                        _detectedAnomalies.Add(alert);
                        
                        await _alertManagerService.CreateAlertAsync(alert);
                    }
                }
                
                return anomalies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting flow execution anomalies");
                return anomalies;
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> DetectResourceAnomaliesAsync()
        {
            var anomalies = new List<AlertModel>();

            try
            {
                var utilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                
                // Check CPU usage
                if (utilization.Cpu.TotalUsagePercent > _thresholds["CPU"])
                {
                    var alert = new AlertModel
                    {
                        Name = "High CPU Usage",
                        Description = $"CPU usage ({utilization.Cpu.TotalUsagePercent:F1}%) exceeds threshold ({_thresholds["CPU"]}%)",
                        Severity = AlertSeverity.Warning,
                        Source = "ResourceMonitor",
                        Category = "Performance",
                        Details = new Dictionary<string, object>
                        {
                            ["CpuUsage"] = utilization.Cpu.TotalUsagePercent,
                            ["Threshold"] = _thresholds["CPU"],
                            ["ProcessorCount"] = utilization.Cpu.ProcessorCount
                        }
                    };
                    
                    anomalies.Add(alert);
                    _detectedAnomalies.Add(alert);
                    
                    await _alertManagerService.CreateAlertAsync(alert);
                }
                
                // Check memory usage
                if (utilization.Memory.UsagePercent > _thresholds["Memory"])
                {
                    var alert = new AlertModel
                    {
                        Name = "High Memory Usage",
                        Description = $"Memory usage ({utilization.Memory.UsagePercent:F1}%) exceeds threshold ({_thresholds["Memory"]}%)",
                        Severity = AlertSeverity.Warning,
                        Source = "ResourceMonitor",
                        Category = "Performance",
                        Details = new Dictionary<string, object>
                        {
                            ["MemoryUsage"] = utilization.Memory.UsagePercent,
                            ["Threshold"] = _thresholds["Memory"],
                            ["TotalMemory"] = utilization.Memory.TotalPhysicalMemory,
                            ["UsedMemory"] = utilization.Memory.UsedPhysicalMemory
                        }
                    };
                    
                    anomalies.Add(alert);
                    _detectedAnomalies.Add(alert);
                    
                    await _alertManagerService.CreateAlertAsync(alert);
                }
                
                // Check disk usage
                foreach (var disk in utilization.Disks)
                {
                    if (disk.UsagePercent > _thresholds["Disk"])
                    {
                        var alert = new AlertModel
                        {
                            Name = "High Disk Usage",
                            Description = $"Disk usage for {disk.DriveName} ({disk.UsagePercent:F1}%) exceeds threshold ({_thresholds["Disk"]}%)",
                            Severity = AlertSeverity.Warning,
                            Source = "ResourceMonitor",
                            Category = "Performance",
                            Details = new Dictionary<string, object>
                            {
                                ["DriveName"] = disk.DriveName,
                                ["DiskUsage"] = disk.UsagePercent,
                                ["Threshold"] = _thresholds["Disk"],
                                ["TotalSpace"] = disk.TotalSize,
                                ["UsedSpace"] = disk.UsedSpace
                            }
                        };
                        
                        anomalies.Add(alert);
                        _detectedAnomalies.Add(alert);
                        
                        await _alertManagerService.CreateAlertAsync(alert);
                    }
                }
                
                return anomalies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting resource anomalies");
                return anomalies;
            }
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetAllAnomaliesAsync()
        {
            return _detectedAnomalies;
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetFlowAnomaliesAsync(string flowId)
        {
            return _detectedAnomalies.Where(a => 
                a.Source == "FlowMonitor" && 
                a.Details.TryGetValue("FlowId", out var id) && 
                id.ToString() == flowId).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<AlertModel>> GetServiceAnomaliesAsync(string serviceId)
        {
            return _detectedAnomalies.Where(a => 
                a.Source == "ServiceMonitor" && 
                a.Details.TryGetValue("ServiceId", out var id) && 
                id.ToString() == serviceId).ToList();
        }
    }
}
