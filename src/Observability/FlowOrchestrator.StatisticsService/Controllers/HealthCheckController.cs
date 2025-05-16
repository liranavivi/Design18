using FlowOrchestrator.Abstractions.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Statistics.Controllers
{
    /// <summary>
    /// Controller for health check operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;
        private readonly IStatisticsLifecycle _statisticsLifecycle;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="statisticsLifecycle">The statistics lifecycle.</param>
        public HealthCheckController(
            ILogger<HealthCheckController> logger,
            IStatisticsLifecycle statisticsLifecycle)
        {
            _logger = logger;
            _statisticsLifecycle = statisticsLifecycle;
        }

        /// <summary>
        /// Gets the health status.
        /// </summary>
        /// <returns>The health status.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult GetHealthStatus()
        {
            try
            {
                var status = _statisticsLifecycle.GetCollectionStatus();
                var isHealthy = status != StatisticsCollectionStatus.ERROR && status != StatisticsCollectionStatus.UNINITIALIZED;
                
                var healthStatus = new
                {
                    Status = isHealthy ? "Healthy" : "Unhealthy",
                    CollectionStatus = status.ToString(),
                    Timestamp = DateTime.UtcNow
                };
                
                if (isHealthy)
                {
                    return Ok(healthStatus);
                }
                else
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, healthStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                
                var errorStatus = new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
                
                return StatusCode(StatusCodes.Status503ServiceUnavailable, errorStatus);
            }
        }

        /// <summary>
        /// Gets detailed health information.
        /// </summary>
        /// <returns>The detailed health information.</returns>
        [HttpGet("details")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetDetailedHealth()
        {
            try
            {
                var status = _statisticsLifecycle.GetCollectionStatus();
                var configuration = _statisticsLifecycle.GetCollectionConfiguration();
                
                var healthDetails = new
                {
                    Status = status.ToString(),
                    Configuration = configuration,
                    ServiceInfo = new
                    {
                        Id = _statisticsLifecycle.LifecycleId,
                        Type = _statisticsLifecycle.LifecycleType,
                        Runtime = Environment.Version.ToString(),
                        OperatingSystem = Environment.OSVersion.ToString(),
                        ProcessorCount = Environment.ProcessorCount,
                        MachineName = Environment.MachineName
                    },
                    Memory = new
                    {
                        TotalMemory = GC.GetTotalMemory(false),
                        WorkingSet = Environment.WorkingSet
                    },
                    Timestamp = DateTime.UtcNow
                };
                
                return Ok(healthDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed health information");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
