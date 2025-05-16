using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Common.Utilities;
using FlowOrchestrator.Observability.Statistics.Models;
using FlowOrchestrator.Observability.Statistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Statistics.Controllers
{
    /// <summary>
    /// Controller for statistics operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> _logger;
        private readonly IStatisticsProvider _statisticsProvider;
        private readonly StatisticsConsumer _statisticsConsumer;
        private readonly IStatisticsLifecycle _statisticsLifecycle;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="statisticsProvider">The statistics provider.</param>
        /// <param name="statisticsConsumer">The statistics consumer.</param>
        /// <param name="statisticsLifecycle">The statistics lifecycle.</param>
        public StatisticsController(
            ILogger<StatisticsController> logger,
            IStatisticsProvider statisticsProvider,
            StatisticsConsumer statisticsConsumer,
            IStatisticsLifecycle statisticsLifecycle)
        {
            _logger = logger;
            _statisticsProvider = statisticsProvider;
            _statisticsConsumer = statisticsConsumer;
            _statisticsLifecycle = statisticsLifecycle;
        }

        /// <summary>
        /// Gets all statistics.
        /// </summary>
        /// <returns>The statistics.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        public IActionResult GetStatistics()
        {
            try
            {
                var statistics = _statisticsProvider.GetStatistics();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific statistic.
        /// </summary>
        /// <param name="key">The statistic key.</param>
        /// <returns>The statistic value.</returns>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStatistic(string key)
        {
            try
            {
                Guard.AgainstNullOrEmpty(key, nameof(key));
                
                var statistics = _statisticsProvider.GetStatistics();
                if (statistics.TryGetValue(key, out var value))
                {
                    return Ok(value);
                }
                
                return NotFound(new { error = $"Statistic with key '{key}' not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistic with key {Key}", key);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Records a metric.
        /// </summary>
        /// <param name="metricRecord">The metric record.</param>
        /// <returns>A success response.</returns>
        [HttpPost("metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RecordMetric([FromBody] MetricRecord metricRecord)
        {
            try
            {
                Guard.AgainstNull(metricRecord, nameof(metricRecord));
                Guard.AgainstNullOrEmpty(metricRecord.Name, nameof(metricRecord.Name));
                
                _statisticsProvider.RecordMetric(metricRecord.Name, metricRecord.Value);
                
                return Ok(new { success = true, message = $"Metric '{metricRecord.Name}' recorded" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        /// <returns>A success response.</returns>
        [HttpPost("reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ResetStatistics()
        {
            try
            {
                _statisticsProvider.ResetStatistics();
                return Ok(new { success = true, message = "Statistics reset" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Queries metrics.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result.</returns>
        [HttpPost("query")]
        [ProducesResponseType(typeof(QueryResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult QueryMetrics([FromBody] MetricQuery query)
        {
            try
            {
                Guard.AgainstNull(query, nameof(query));
                
                var result = _statisticsConsumer.QueryMetrics(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying metrics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the available metrics.
        /// </summary>
        /// <returns>The available metrics.</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(List<MetricDefinition>), StatusCodes.Status200OK)]
        public IActionResult GetAvailableMetrics()
        {
            try
            {
                var metrics = _statisticsConsumer.GetAvailableMetrics();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available metrics");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the configured alerts.
        /// </summary>
        /// <returns>The configured alerts.</returns>
        [HttpGet("alerts")]
        [ProducesResponseType(typeof(List<AlertDefinition>), StatusCodes.Status200OK)]
        public IActionResult GetConfiguredAlerts()
        {
            try
            {
                var alerts = _statisticsConsumer.GetConfiguredAlerts();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configured alerts");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the collection status.
        /// </summary>
        /// <returns>The collection status.</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(StatisticsCollectionStatus), StatusCodes.Status200OK)]
        public IActionResult GetCollectionStatus()
        {
            try
            {
                var status = _statisticsLifecycle.GetCollectionStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection status");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the collection configuration.
        /// </summary>
        /// <returns>The collection configuration.</returns>
        [HttpGet("configuration")]
        [ProducesResponseType(typeof(StatisticsCollectionConfiguration), StatusCodes.Status200OK)]
        public IActionResult GetCollectionConfiguration()
        {
            try
            {
                var configuration = _statisticsLifecycle.GetCollectionConfiguration();
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sets the collection configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A success response.</returns>
        [HttpPost("configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetCollectionConfiguration([FromBody] StatisticsCollectionConfiguration configuration)
        {
            try
            {
                Guard.AgainstNull(configuration, nameof(configuration));
                
                _statisticsLifecycle.SetCollectionConfiguration(configuration);
                return Ok(new { success = true, message = "Collection configuration set" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting collection configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Starts the statistics collection.
        /// </summary>
        /// <returns>A success response.</returns>
        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult StartCollection()
        {
            try
            {
                _statisticsLifecycle.StartCollection();
                return Ok(new { success = true, message = "Statistics collection started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting statistics collection");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Stops the statistics collection.
        /// </summary>
        /// <returns>A success response.</returns>
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult StopCollection()
        {
            try
            {
                _statisticsLifecycle.StopCollection();
                return Ok(new { success = true, message = "Statistics collection stopped" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping statistics collection");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Pauses the statistics collection.
        /// </summary>
        /// <returns>A success response.</returns>
        [HttpPost("pause")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PauseCollection()
        {
            try
            {
                _statisticsLifecycle.PauseCollection();
                return Ok(new { success = true, message = "Statistics collection paused" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing statistics collection");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resumes the statistics collection.
        /// </summary>
        /// <returns>A success response.</returns>
        [HttpPost("resume")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ResumeCollection()
        {
            try
            {
                _statisticsLifecycle.ResumeCollection();
                return Ok(new { success = true, message = "Statistics collection resumed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming statistics collection");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
