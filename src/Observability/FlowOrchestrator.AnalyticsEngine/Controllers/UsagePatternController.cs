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
    /// Controller for usage pattern analytics.
    /// </summary>
    [ApiController]
    [Route("api/analytics/usage")]
    public class UsagePatternController : ControllerBase
    {
        private readonly ILogger<UsagePatternController> _logger;
        private readonly IUsagePatternService _usagePatternService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsagePatternController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="usagePatternService">The usage pattern service.</param>
        public UsagePatternController(
            ILogger<UsagePatternController> logger,
            IUsagePatternService usagePatternService)
        {
            _logger = logger;
            _usagePatternService = usagePatternService;
        }

        /// <summary>
        /// Gets a usage pattern analysis.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The usage pattern analysis result.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsagePatternAnalysisResult>> GetUsagePatternAnalysisAsync(
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

                var result = await _usagePatternService.AnalyzeUsagePatternsAsync(timeRange);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage pattern analysis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting usage pattern analysis");
            }
        }

        /// <summary>
        /// Gets flow usage patterns.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of flow usage patterns.</returns>
        [HttpGet("flows")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<FlowUsagePattern>>> GetFlowUsagePatternsAsync(
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

                var patterns = await _usagePatternService.AnalyzeFlowUsagePatternsAsync(timeRange);
                return Ok(patterns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flow usage patterns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting flow usage patterns");
            }
        }

        /// <summary>
        /// Gets component usage patterns.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of component usage patterns.</returns>
        [HttpGet("components")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ComponentUsagePattern>>> GetComponentUsagePatternsAsync(
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

                var patterns = await _usagePatternService.AnalyzeComponentUsagePatternsAsync(timeRange);
                return Ok(patterns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting component usage patterns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting component usage patterns");
            }
        }

        /// <summary>
        /// Gets resource usage patterns.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of resource usage patterns.</returns>
        [HttpGet("resources")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ResourceUsagePattern>>> GetResourceUsagePatternsAsync(
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

                var patterns = await _usagePatternService.AnalyzeResourceUsagePatternsAsync(timeRange);
                return Ok(patterns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource usage patterns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting resource usage patterns");
            }
        }

        /// <summary>
        /// Gets temporal patterns.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of temporal patterns.</returns>
        [HttpGet("temporal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TemporalPattern>>> GetTemporalPatternsAsync(
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

                var patterns = await _usagePatternService.AnalyzeTemporalPatternsAsync(timeRange);
                return Ok(patterns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting temporal patterns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting temporal patterns");
            }
        }
    }
}
