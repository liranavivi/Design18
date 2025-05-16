using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Monitoring.Controllers
{
    /// <summary>
    /// Controller for alert operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly ILogger<AlertsController> _logger;
        private readonly IAlertManagerService _alertManagerService;
        private readonly IPerformanceAnomalyDetectorService _performanceAnomalyDetectorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="alertManagerService">The alert manager service.</param>
        /// <param name="performanceAnomalyDetectorService">The performance anomaly detector service.</param>
        public AlertsController(
            ILogger<AlertsController> logger,
            IAlertManagerService alertManagerService,
            IPerformanceAnomalyDetectorService performanceAnomalyDetectorService)
        {
            _logger = logger;
            _alertManagerService = alertManagerService;
            _performanceAnomalyDetectorService = performanceAnomalyDetectorService;
        }

        /// <summary>
        /// Gets all alerts.
        /// </summary>
        /// <param name="includeResolved">Whether to include resolved alerts.</param>
        /// <returns>The list of alerts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlerts([FromQuery] bool includeResolved = false)
        {
            try
            {
                var alerts = await _alertManagerService.GetAlertsAsync(includeResolved);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets alerts by severity.
        /// </summary>
        /// <param name="severity">The alert severity.</param>
        /// <returns>The list of alerts.</returns>
        [HttpGet("severity/{severity}")]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlertsBySeverity(AlertSeverity severity)
        {
            try
            {
                var alerts = await _alertManagerService.GetAlertsBySeverityAsync(severity);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts by severity {Severity}", severity);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets alerts by category.
        /// </summary>
        /// <param name="category">The alert category.</param>
        /// <returns>The list of alerts.</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlertsByCategory(string category)
        {
            try
            {
                var alerts = await _alertManagerService.GetAlertsByCategoryAsync(category);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts by category {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets alerts by source.
        /// </summary>
        /// <param name="source">The alert source.</param>
        /// <returns>The list of alerts.</returns>
        [HttpGet("source/{source}")]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlertsBySource(string source)
        {
            try
            {
                var alerts = await _alertManagerService.GetAlertsBySourceAsync(source);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts by source {Source}", source);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets an alert by ID.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <returns>The alert model.</returns>
        [HttpGet("{alertId}")]
        [ProducesResponseType(typeof(AlertModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlertById(string alertId)
        {
            try
            {
                var alert = await _alertManagerService.GetAlertByIdAsync(alertId);
                
                if (alert == null)
                {
                    return NotFound(new { error = $"Alert with ID {alertId} not found" });
                }
                
                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert {AlertId}", alertId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Creates an alert.
        /// </summary>
        /// <param name="alert">The alert model.</param>
        /// <returns>The created alert.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AlertModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAlert([FromBody] AlertModel alert)
        {
            try
            {
                if (string.IsNullOrEmpty(alert.Name) || string.IsNullOrEmpty(alert.Description))
                {
                    return BadRequest(new { error = "Name and Description are required" });
                }

                var createdAlert = await _alertManagerService.CreateAlertAsync(alert);
                return CreatedAtAction(nameof(GetAlertById), new { alertId = createdAlert.AlertId }, createdAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Acknowledges an alert.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <param name="acknowledgedBy">The user who acknowledged the alert.</param>
        /// <returns>The acknowledged alert.</returns>
        [HttpPost("{alertId}/acknowledge")]
        [ProducesResponseType(typeof(AlertModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcknowledgeAlert(string alertId, [FromQuery] string acknowledgedBy)
        {
            try
            {
                var alert = await _alertManagerService.AcknowledgeAlertAsync(alertId, acknowledgedBy);
                
                if (alert == null)
                {
                    return NotFound(new { error = $"Alert with ID {alertId} not found" });
                }
                
                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resolves an alert.
        /// </summary>
        /// <param name="alertId">The alert identifier.</param>
        /// <param name="resolutionNotes">The resolution notes.</param>
        /// <returns>The resolved alert.</returns>
        [HttpPost("{alertId}/resolve")]
        [ProducesResponseType(typeof(AlertModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResolveAlert(string alertId, [FromQuery] string resolutionNotes)
        {
            try
            {
                var alert = await _alertManagerService.ResolveAlertAsync(alertId, resolutionNotes);
                
                if (alert == null)
                {
                    return NotFound(new { error = $"Alert with ID {alertId} not found" });
                }
                
                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Detects resource anomalies.
        /// </summary>
        /// <returns>The list of alerts for detected anomalies.</returns>
        [HttpPost("detect/resource")]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DetectResourceAnomalies()
        {
            try
            {
                var anomalies = await _performanceAnomalyDetectorService.DetectResourceAnomaliesAsync();
                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting resource anomalies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Detects flow execution anomalies.
        /// </summary>
        /// <returns>The list of alerts for detected anomalies.</returns>
        [HttpPost("detect/flow")]
        [ProducesResponseType(typeof(List<AlertModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DetectFlowExecutionAnomalies()
        {
            try
            {
                var anomalies = await _performanceAnomalyDetectorService.DetectFlowExecutionAnomaliesAsync();
                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting flow execution anomalies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
