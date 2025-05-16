using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowOrchestrator.Observability.Monitoring.Controllers
{
    /// <summary>
    /// Controller for health check operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="healthCheckService">The health check service.</param>
        public HealthCheckController(
            ILogger<HealthCheckController> logger,
            IHealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Gets the health status of all services.
        /// </summary>
        /// <returns>The system status response.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SystemStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                var status = await _healthCheckService.GetSystemHealthStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the health status of a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service health status.</returns>
        [HttpGet("service/{serviceId}")]
        [ProducesResponseType(typeof(ServiceHealthStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceHealthStatus(string serviceId)
        {
            try
            {
                var status = await _healthCheckService.GetServiceHealthStatusAsync(serviceId);
                return Ok(status);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Service with ID {serviceId} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status for service {ServiceId}", serviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Discovers available services.
        /// </summary>
        /// <returns>The list of service health statuses.</returns>
        [HttpGet("discover")]
        [ProducesResponseType(typeof(List<ServiceHealthStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DiscoverServices()
        {
            try
            {
                var services = await _healthCheckService.DiscoverServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering services");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Registers a service for health monitoring.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="endpoint">The service endpoint.</param>
        /// <returns>A success message.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RegisterService(string serviceId, string serviceType, string endpoint)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(serviceType) || string.IsNullOrEmpty(endpoint))
                {
                    return BadRequest(new { error = "ServiceId, ServiceType, and Endpoint are required" });
                }

                var result = _healthCheckService.RegisterService(serviceId, serviceType, endpoint);

                if (result)
                {
                    return Ok(new { message = $"Service {serviceId} registered successfully" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to register service" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering service {ServiceId}", serviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Unregisters a service from health monitoring.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>A success message.</returns>
        [HttpPost("unregister")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UnregisterService(string serviceId)
        {
            try
            {
                var result = _healthCheckService.UnregisterService(serviceId);

                if (result)
                {
                    return Ok(new { message = $"Service {serviceId} unregistered successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Service with ID {serviceId} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering service {ServiceId}", serviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
