using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Monitoring.Controllers
{
    /// <summary>
    /// Controller for system status operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SystemStatusController : ControllerBase
    {
        private readonly ILogger<SystemStatusController> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IResourceUtilizationMonitorService _resourceUtilizationMonitorService;
        private readonly IActiveFlowMonitorService _activeFlowMonitorService;
        private readonly IAlertManagerService _alertManagerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemStatusController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="healthCheckService">The health check service.</param>
        /// <param name="resourceUtilizationMonitorService">The resource utilization monitor service.</param>
        /// <param name="activeFlowMonitorService">The active flow monitor service.</param>
        /// <param name="alertManagerService">The alert manager service.</param>
        public SystemStatusController(
            ILogger<SystemStatusController> logger,
            IHealthCheckService healthCheckService,
            IResourceUtilizationMonitorService resourceUtilizationMonitorService,
            IActiveFlowMonitorService activeFlowMonitorService,
            IAlertManagerService alertManagerService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
            _resourceUtilizationMonitorService = resourceUtilizationMonitorService;
            _activeFlowMonitorService = activeFlowMonitorService;
            _alertManagerService = alertManagerService;
        }

        /// <summary>
        /// Gets the system status dashboard data.
        /// </summary>
        /// <returns>The system status dashboard data.</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSystemStatusDashboard()
        {
            try
            {
                // Get system health status
                var healthStatus = await _healthCheckService.GetSystemHealthStatusAsync();
                
                // Get resource utilization
                var resourceUtilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                
                // Get active flows
                var activeFlows = await _activeFlowMonitorService.GetActiveFlowsAsync();
                
                // Get active alerts
                var activeAlerts = await _alertManagerService.GetAlertsAsync(false);
                
                var dashboardData = new
                {
                    SystemStatus = healthStatus.Status,
                    ServiceCount = healthStatus.Services.Count,
                    HealthyServiceCount = healthStatus.Services.Count(s => s.HealthStatus == "Healthy"),
                    DegradedServiceCount = healthStatus.Services.Count(s => s.HealthStatus == "Degraded"),
                    UnhealthyServiceCount = healthStatus.Services.Count(s => s.HealthStatus == "Unhealthy"),
                    ResourceUtilization = new
                    {
                        CpuUsage = resourceUtilization.Cpu.TotalUsagePercent,
                        MemoryUsage = resourceUtilization.Memory.UsagePercent,
                        DiskUsage = resourceUtilization.Disks.Select(d => new { d.DriveName, d.UsagePercent }).ToList()
                    },
                    ActiveFlows = new
                    {
                        TotalCount = activeFlows.Count,
                        RunningCount = activeFlows.Count(f => f.ExecutionStatus == "Running"),
                        PausedCount = activeFlows.Count(f => f.ExecutionStatus == "Paused"),
                        ErrorCount = activeFlows.Count(f => f.ExecutionStatus == "Error")
                    },
                    Alerts = new
                    {
                        TotalCount = activeAlerts.Count,
                        CriticalCount = activeAlerts.Count(a => a.Severity == AlertSeverity.Critical),
                        ErrorCount = activeAlerts.Count(a => a.Severity == AlertSeverity.Error),
                        WarningCount = activeAlerts.Count(a => a.Severity == AlertSeverity.Warning),
                        InfoCount = activeAlerts.Count(a => a.Severity == AlertSeverity.Info)
                    },
                    Timestamp = DateTime.UtcNow
                };
                
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system status dashboard");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the system overview.
        /// </summary>
        /// <returns>The system overview.</returns>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSystemOverview()
        {
            try
            {
                // Get system health status
                var healthStatus = await _healthCheckService.GetSystemHealthStatusAsync();
                
                // Get resource utilization
                var resourceUtilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                
                var overview = new
                {
                    Status = healthStatus.Status,
                    ResourceUtilization = new
                    {
                        CpuUsage = resourceUtilization.Cpu.TotalUsagePercent,
                        MemoryUsage = resourceUtilization.Memory.UsagePercent,
                        DiskUsage = resourceUtilization.Disks.Select(d => new { d.DriveName, d.UsagePercent }).ToList()
                    },
                    Services = healthStatus.Services.Select(s => new
                    {
                        s.ServiceId,
                        s.ServiceType,
                        s.Version,
                        s.Status,
                        s.HealthStatus
                    }).ToList(),
                    Host = resourceUtilization.Host,
                    Timestamp = DateTime.UtcNow
                };
                
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system overview");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
