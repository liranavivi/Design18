using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowOrchestrator.Observability.Analytics.Models;
using FlowOrchestrator.Observability.Analytics.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Observability.Analytics.Controllers
{
    /// <summary>
    /// Controller for performance analytics.
    /// </summary>
    [ApiController]
    [Route("api/analytics/performance")]
    public class PerformanceAnalyticsController : ControllerBase
    {
        private readonly ILogger<PerformanceAnalyticsController> _logger;
        private readonly IPerformanceAnalyticsService _performanceAnalyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyticsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="performanceAnalyticsService">The performance analytics service.</param>
        public PerformanceAnalyticsController(
            ILogger<PerformanceAnalyticsController> logger,
            IPerformanceAnalyticsService performanceAnalyticsService)
        {
            _logger = logger;
            _performanceAnalyticsService = performanceAnalyticsService;
        }

        /// <summary>
        /// Gets a performance analysis.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The performance analysis result.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PerformanceAnalysisResult>> GetPerformanceAnalysisAsync(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            try
            {
                TimeRange? timeRange = null;
                if (startTime.HasValue || endTime.HasValue)
                {
                    timeRange = new TimeRange
                    {
                        StartTime = startTime ?? DateTime.UtcNow.AddHours(-24),
                        EndTime = endTime ?? DateTime.UtcNow
                    };

                    if (timeRange.StartTime >= timeRange.EndTime)
                    {
                        return BadRequest("Start time must be before end time");
                    }
                }

                var result = await _performanceAnalyticsService.AnalyzePerformanceAsync(timeRange);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance analysis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting performance analysis");
            }
        }

        /// <summary>
        /// Gets performance metrics for a specific category.
        /// </summary>
        /// <param name="category">The metric category.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of performance metrics.</returns>
        [HttpGet("metrics/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PerformanceMetric>>> GetPerformanceMetricsAsync(
            string category,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            if (string.IsNullOrEmpty(category))
            {
                return BadRequest("Category is required");
            }

            try
            {
                TimeRange? timeRange = null;
                if (startTime.HasValue || endTime.HasValue)
                {
                    timeRange = new TimeRange
                    {
                        StartTime = startTime ?? DateTime.UtcNow.AddHours(-24),
                        EndTime = endTime ?? DateTime.UtcNow
                    };

                    if (timeRange.StartTime >= timeRange.EndTime)
                    {
                        return BadRequest("Start time must be before end time");
                    }
                }

                var metrics = await _performanceAnalyticsService.GetPerformanceMetricsAsync(category, timeRange);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for category {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting performance metrics");
            }
        }

        /// <summary>
        /// Gets performance trends.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of performance trends.</returns>
        [HttpGet("trends")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PerformanceTrend>>> GetPerformanceTrendsAsync(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            try
            {
                TimeRange? timeRange = null;
                if (startTime.HasValue || endTime.HasValue)
                {
                    timeRange = new TimeRange
                    {
                        StartTime = startTime ?? DateTime.UtcNow.AddHours(-24),
                        EndTime = endTime ?? DateTime.UtcNow
                    };

                    if (timeRange.StartTime >= timeRange.EndTime)
                    {
                        return BadRequest("Start time must be before end time");
                    }
                }

                var analysis = await _performanceAnalyticsService.AnalyzePerformanceAsync(timeRange);
                return Ok(analysis.Trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance trends");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting performance trends");
            }
        }

        /// <summary>
        /// Gets performance anomalies.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of performance anomalies.</returns>
        [HttpGet("anomalies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PerformanceAnomaly>>> GetPerformanceAnomaliesAsync(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            try
            {
                TimeRange? timeRange = null;
                if (startTime.HasValue || endTime.HasValue)
                {
                    timeRange = new TimeRange
                    {
                        StartTime = startTime ?? DateTime.UtcNow.AddHours(-24),
                        EndTime = endTime ?? DateTime.UtcNow
                    };

                    if (timeRange.StartTime >= timeRange.EndTime)
                    {
                        return BadRequest("Start time must be before end time");
                    }
                }

                var analysis = await _performanceAnalyticsService.AnalyzePerformanceAsync(timeRange);
                return Ok(analysis.Anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance anomalies");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting performance anomalies");
            }
        }

        /// <summary>
        /// Gets performance correlations.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of performance correlations.</returns>
        [HttpGet("correlations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PerformanceCorrelation>>> GetPerformanceCorrelationsAsync(
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            try
            {
                TimeRange? timeRange = null;
                if (startTime.HasValue || endTime.HasValue)
                {
                    timeRange = new TimeRange
                    {
                        StartTime = startTime ?? DateTime.UtcNow.AddHours(-24),
                        EndTime = endTime ?? DateTime.UtcNow
                    };

                    if (timeRange.StartTime >= timeRange.EndTime)
                    {
                        return BadRequest("Start time must be before end time");
                    }
                }

                var analysis = await _performanceAnalyticsService.AnalyzePerformanceAsync(timeRange);
                return Ok(analysis.Correlations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance correlations");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting performance correlations");
            }
        }
    }
}
