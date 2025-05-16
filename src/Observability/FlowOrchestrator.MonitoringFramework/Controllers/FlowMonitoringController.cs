using FlowOrchestrator.Observability.Monitoring.Models;
using FlowOrchestrator.Observability.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.Observability.Monitoring.Controllers
{
    /// <summary>
    /// Controller for flow monitoring operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlowMonitoringController : ControllerBase
    {
        private readonly ILogger<FlowMonitoringController> _logger;
        private readonly IActiveFlowMonitorService _activeFlowMonitorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowMonitoringController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="activeFlowMonitorService">The active flow monitor service.</param>
        public FlowMonitoringController(
            ILogger<FlowMonitoringController> logger,
            IActiveFlowMonitorService activeFlowMonitorService)
        {
            _logger = logger;
            _activeFlowMonitorService = activeFlowMonitorService;
        }

        /// <summary>
        /// Gets all active flows.
        /// </summary>
        /// <returns>The list of active flow statuses.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ActiveFlowStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveFlows()
        {
            try
            {
                var flows = await _activeFlowMonitorService.GetActiveFlowsAsync();
                return Ok(flows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active flows");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets an active flow by ID.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The active flow status.</returns>
        [HttpGet("{flowId}/{executionId}")]
        [ProducesResponseType(typeof(ActiveFlowStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveFlow(string flowId, string executionId)
        {
            try
            {
                var flow = await _activeFlowMonitorService.GetActiveFlowAsync(flowId, executionId);
                
                if (flow == null)
                {
                    return NotFound(new { error = $"Flow execution {executionId} for flow {flowId} not found" });
                }
                
                return Ok(flow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active flow {FlowId}/{ExecutionId}", flowId, executionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets active flows by status.
        /// </summary>
        /// <param name="status">The execution status.</param>
        /// <returns>The list of active flow statuses.</returns>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(List<ActiveFlowStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveFlowsByStatus(string status)
        {
            try
            {
                var flows = await _activeFlowMonitorService.GetActiveFlowsByStatusAsync(status);
                return Ok(flows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active flows with status {Status}", status);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets flow execution history.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="limit">The maximum number of executions to retrieve.</param>
        /// <returns>The list of active flow statuses.</returns>
        [HttpGet("{flowId}/history")]
        [ProducesResponseType(typeof(List<ActiveFlowStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowExecutionHistory(string flowId, [FromQuery] int limit = 10)
        {
            try
            {
                var history = await _activeFlowMonitorService.GetFlowExecutionHistoryAsync(flowId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flow execution history for flow {FlowId}", flowId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets flow execution statistics.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <returns>The dictionary of statistics.</returns>
        [HttpGet("{flowId}/statistics")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlowExecutionStatistics(string flowId)
        {
            try
            {
                var statistics = await _activeFlowMonitorService.GetFlowExecutionStatisticsAsync(flowId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flow execution statistics for flow {FlowId}", flowId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Registers a flow execution for monitoring.
        /// </summary>
        /// <param name="flowStatus">The active flow status.</param>
        /// <returns>A success message.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RegisterFlowExecution([FromBody] ActiveFlowStatus flowStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(flowStatus.FlowId) || string.IsNullOrEmpty(flowStatus.ExecutionId))
                {
                    return BadRequest(new { error = "FlowId and ExecutionId are required" });
                }

                _activeFlowMonitorService.RegisterFlowExecution(flowStatus);
                return Ok(new { message = $"Flow execution {flowStatus.ExecutionId} for flow {flowStatus.FlowId} registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering flow execution {ExecutionId} for flow {FlowId}", 
                    flowStatus.ExecutionId, flowStatus.FlowId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a flow execution status.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="status">The execution status.</param>
        /// <param name="currentStep">The current step.</param>
        /// <returns>A success message.</returns>
        [HttpPost("{flowId}/{executionId}/update")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateFlowExecutionStatus(string flowId, string executionId, [FromQuery] string status, [FromQuery] string currentStep)
        {
            try
            {
                _activeFlowMonitorService.UpdateFlowExecutionStatus(flowId, executionId, status, currentStep);
                return Ok(new { message = $"Flow execution {executionId} for flow {flowId} updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flow execution {ExecutionId} for flow {FlowId}", 
                    executionId, flowId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
