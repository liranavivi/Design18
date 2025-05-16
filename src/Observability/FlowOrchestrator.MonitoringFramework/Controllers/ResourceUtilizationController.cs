using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Monitoring.Controllers
{
    /// <summary>
    /// Controller for resource utilization operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceUtilizationController : ControllerBase
    {
        private readonly ILogger<ResourceUtilizationController> _logger;
        private readonly IResourceUtilizationMonitorService _resourceUtilizationMonitorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceUtilizationController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="resourceUtilizationMonitorService">The resource utilization monitor service.</param>
        public ResourceUtilizationController(
            ILogger<ResourceUtilizationController> logger,
            IResourceUtilizationMonitorService resourceUtilizationMonitorService)
        {
            _logger = logger;
            _resourceUtilizationMonitorService = resourceUtilizationMonitorService;
        }

        /// <summary>
        /// Gets the current resource utilization.
        /// </summary>
        /// <returns>The resource utilization response.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResourceUtilizationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResourceUtilization()
        {
            try
            {
                var utilization = await _resourceUtilizationMonitorService.GetResourceUtilizationAsync();
                return Ok(utilization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource utilization");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the resource utilization history.
        /// </summary>
        /// <param name="minutes">The number of minutes to retrieve history for.</param>
        /// <param name="intervalSeconds">The interval between data points in seconds.</param>
        /// <returns>The list of resource utilization responses.</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<ResourceUtilizationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResourceUtilizationHistory(int minutes = 60, int intervalSeconds = 60)
        {
            try
            {
                var history = await _resourceUtilizationMonitorService.GetResourceUtilizationHistoryAsync(
                    TimeSpan.FromMinutes(minutes), intervalSeconds);
                
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource utilization history");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the resource utilization for a specific service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service resource utilization.</returns>
        [HttpGet("service/{serviceId}")]
        [ProducesResponseType(typeof(ServiceResourceUtilization), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetServiceResourceUtilization(string serviceId)
        {
            try
            {
                var utilization = await _resourceUtilizationMonitorService.GetServiceResourceUtilizationAsync(serviceId);
                return Ok(utilization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource utilization for service {ServiceId}", serviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the resource utilization thresholds.
        /// </summary>
        /// <returns>The dictionary of resource thresholds.</returns>
        [HttpGet("thresholds")]
        [ProducesResponseType(typeof(Dictionary<string, double>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetResourceThresholds()
        {
            try
            {
                var thresholds = _resourceUtilizationMonitorService.GetResourceThresholds();
                return Ok(thresholds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource thresholds");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sets a resource utilization threshold.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <param name="thresholdPercent">The threshold percentage.</param>
        /// <returns>A success message.</returns>
        [HttpPost("thresholds")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult SetResourceThreshold(string resourceName, double thresholdPercent)
        {
            try
            {
                if (string.IsNullOrEmpty(resourceName))
                {
                    return BadRequest(new { error = "ResourceName is required" });
                }

                if (thresholdPercent < 0 || thresholdPercent > 100)
                {
                    return BadRequest(new { error = "ThresholdPercent must be between 0 and 100" });
                }

                _resourceUtilizationMonitorService.SetResourceThreshold(resourceName, thresholdPercent);
                return Ok(new { message = $"Resource threshold for {resourceName} set to {thresholdPercent}%" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting resource threshold for {ResourceName}", resourceName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
