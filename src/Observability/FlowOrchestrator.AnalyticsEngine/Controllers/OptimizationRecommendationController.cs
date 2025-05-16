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
    /// Controller for optimization recommendations.
    /// </summary>
    [ApiController]
    [Route("api/analytics/recommendations")]
    public class OptimizationRecommendationController : ControllerBase
    {
        private readonly ILogger<OptimizationRecommendationController> _logger;
        private readonly IOptimizationRecommendationService _recommendationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizationRecommendationController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="recommendationService">The recommendation service.</param>
        public OptimizationRecommendationController(
            ILogger<OptimizationRecommendationController> logger,
            IOptimizationRecommendationService recommendationService)
        {
            _logger = logger;
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Gets optimization recommendations.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The optimization recommendation result.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OptimizationRecommendationResult>> GetRecommendationsAsync(
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

                var result = await _recommendationService.GenerateRecommendationsAsync(timeRange);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimization recommendations");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting optimization recommendations");
            }
        }

        /// <summary>
        /// Gets recommendations for a specific entity.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of recommendations.</returns>
        [HttpGet("entity/{entityType}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OptimizationRecommendation>>> GetRecommendationsByEntityAsync(
            string entityType,
            string entityId,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId))
            {
                return BadRequest("Entity type and entity ID are required");
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

                var recommendations = await _recommendationService.GetRecommendationsByEntityAsync(entityType, entityId, timeRange);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for entity {EntityType}/{EntityId}", entityType, entityId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting recommendations");
            }
        }

        /// <summary>
        /// Gets recommendations for a specific category.
        /// </summary>
        /// <param name="category">The recommendation category.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The list of recommendations.</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OptimizationRecommendation>>> GetRecommendationsByCategoryAsync(
            string category,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            if (string.IsNullOrEmpty(category) || !Enum.TryParse<RecommendationCategory>(category, true, out var recommendationCategory))
            {
                return BadRequest("Valid category is required");
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

                var recommendations = await _recommendationService.GetRecommendationsByCategoryAsync(recommendationCategory, timeRange);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for category {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting recommendations");
            }
        }
    }
}
