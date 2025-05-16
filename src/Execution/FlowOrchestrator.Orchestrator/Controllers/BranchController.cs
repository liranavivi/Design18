using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Orchestrator.Models;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using static FlowOrchestrator.Orchestrator.Models.ExecutionErrorExtensions;

namespace FlowOrchestrator.Orchestrator.Controllers
{
    /// <summary>
    /// Controller for branch management operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly ILogger<BranchController> _logger;
        private readonly BranchManagementService _branchManagementService;
        private readonly TelemetryService _telemetryService;
        private readonly ErrorHandlingService _errorHandlingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchManagementService">The branch management service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        /// <param name="errorHandlingService">The error handling service.</param>
        public BranchController(
            ILogger<BranchController> logger,
            BranchManagementService branchManagementService,
            TelemetryService telemetryService,
            ErrorHandlingService errorHandlingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchManagementService = branchManagementService ?? throw new ArgumentNullException(nameof(branchManagementService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
        }

        /// <summary>
        /// Gets all branches for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The branch execution contexts.</returns>
        [HttpGet("execution/{executionId}")]
        [ProducesResponseType(typeof(IEnumerable<BranchStatusInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBranchesForExecution(string executionId)
        {
            try
            {
                _logger.LogInformation("Received request to get branches for execution {ExecutionId}", executionId);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "GetBranchesForExecution", "CONTROLLER", "BranchController");

                var branches = await _branchManagementService.GetBranchesForExecutionAsync(executionId);
                if (!branches.Any())
                {
                    return NotFound(new { message = $"No branches found for execution {executionId}" });
                }

                var response = branches.Select(b => new BranchStatusInfo
                {
                    BranchId = b.BranchId,
                    Status = b.Status.ToString(),
                    CompletedSteps = b.CompletedSteps.Count,
                    PendingSteps = b.PendingSteps.Count
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches for execution {ExecutionId}", executionId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "BranchController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "GetBranchesForExecution", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext { ExecutionId = executionId });

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }

        /// <summary>
        /// Gets a specific branch for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch execution context.</returns>
        [HttpGet("execution/{executionId}/branch/{branchId}")]
        [ProducesResponseType(typeof(BranchExecutionContext), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBranch(string executionId, string branchId)
        {
            try
            {
                _logger.LogInformation("Received request to get branch {BranchId} for execution {ExecutionId}", branchId, executionId);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "GetBranch", "CONTROLLER", "BranchController");

                var branch = await _branchManagementService.GetBranchAsync(executionId, branchId);
                if (branch == null)
                {
                    return NotFound(new { message = $"Branch {branchId} not found for execution {executionId}" });
                }

                return Ok(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch {BranchId} for execution {ExecutionId}", branchId, executionId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "BranchController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "GetBranch", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext { ExecutionId = executionId });

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }

        /// <summary>
        /// Updates the status of a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="status">The new status.</param>
        /// <returns>The result of the operation.</returns>
        [HttpPut("execution/{executionId}/branch/{branchId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBranchStatus(string executionId, string branchId, [FromBody] string status)
        {
            try
            {
                _logger.LogInformation("Received request to update branch {BranchId} status to {Status}", branchId, status);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "UpdateBranchStatus", "CONTROLLER", "BranchController");

                if (!Enum.TryParse<BranchStatus>(status, true, out var branchStatus))
                {
                    return BadRequest(new { message = $"Invalid branch status: {status}" });
                }

                var result = await _branchManagementService.UpdateBranchStatusAsync(executionId, branchId, branchStatus);
                if (!result)
                {
                    return NotFound(new { message = $"Branch {branchId} not found for execution {executionId}" });
                }

                return Ok(new { message = $"Branch {branchId} status updated to {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch {BranchId} status", branchId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "BranchController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "UpdateBranchStatus", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext { ExecutionId = executionId });

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }
    }
}
