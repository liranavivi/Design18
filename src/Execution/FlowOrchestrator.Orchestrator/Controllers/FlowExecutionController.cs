using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Orchestrator.Models;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using static FlowOrchestrator.Orchestrator.Models.ExecutionErrorExtensions;

namespace FlowOrchestrator.Orchestrator.Controllers
{
    /// <summary>
    /// Controller for flow execution operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlowExecutionController : ControllerBase
    {
        private readonly ILogger<FlowExecutionController> _logger;
        private readonly OrchestratorService _orchestratorService;
        private readonly BranchManagementService _branchManagementService;
        private readonly MemoryAddressService _memoryAddressService;
        private readonly MergeStrategyService _mergeStrategyService;
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly TelemetryService _telemetryService;
        private readonly IExecutionRepository _executionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="orchestratorService">The orchestrator service.</param>
        /// <param name="branchManagementService">The branch management service.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        /// <param name="mergeStrategyService">The merge strategy service.</param>
        /// <param name="errorHandlingService">The error handling service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        /// <param name="executionRepository">The execution repository.</param>
        public FlowExecutionController(
            ILogger<FlowExecutionController> logger,
            OrchestratorService orchestratorService,
            BranchManagementService branchManagementService,
            MemoryAddressService memoryAddressService,
            MergeStrategyService mergeStrategyService,
            ErrorHandlingService errorHandlingService,
            TelemetryService telemetryService,
            IExecutionRepository executionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orchestratorService = orchestratorService ?? throw new ArgumentNullException(nameof(orchestratorService));
            _branchManagementService = branchManagementService ?? throw new ArgumentNullException(nameof(branchManagementService));
            _memoryAddressService = memoryAddressService ?? throw new ArgumentNullException(nameof(memoryAddressService));
            _mergeStrategyService = mergeStrategyService ?? throw new ArgumentNullException(nameof(mergeStrategyService));
            _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        }

        /// <summary>
        /// Starts a flow execution.
        /// </summary>
        /// <param name="request">The start flow request.</param>
        /// <returns>The execution context.</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(FlowOrchestrator.Abstractions.Common.ExecutionContext), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartFlow([FromBody] StartFlowRequest request)
        {
            try
            {
                _logger.LogInformation("Received request to start flow {FlowId}", request.FlowId);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "StartFlow", "CONTROLLER", "FlowExecutionController");

                // In a real implementation, we would retrieve the flow entity from a repository
                // For now, we'll create a mock flow entity
                var flowEntity = new MockFlowEntity
                {
                    FlowId = request.FlowId,
                    Version = request.FlowVersion ?? "1.0.0",
                    ImporterServiceId = request.ImporterServiceId ?? "MOCK-IMPORTER",
                    ProcessorServiceIds = request.ProcessorServiceIds ?? new List<string> { "MOCK-PROCESSOR" },
                    ExporterServiceIds = request.ExporterServiceIds ?? new List<string> { "MOCK-EXPORTER" }
                };

                // Start the flow execution
                var executionContext = await _orchestratorService.StartFlowExecutionAsync(flowEntity, request.Parameters ?? new Dictionary<string, object>());

                return Ok(executionContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting flow {FlowId}", request.FlowId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "FlowExecutionController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "StartFlow", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext());

                return BadRequest(new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }

        /// <summary>
        /// Gets the status of a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The execution status.</returns>
        [HttpGet("{executionId}/status")]
        [ProducesResponseType(typeof(ExecutionStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExecutionStatus(string executionId)
        {
            try
            {
                _logger.LogInformation("Received request to get status for execution {ExecutionId}", executionId);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "GetExecutionStatus", "CONTROLLER", "FlowExecutionController");

                // Get execution context from repository
                var executionContext = await _executionRepository.GetExecutionContextAsync(executionId);
                if (executionContext == null)
                {
                    return NotFound(new { error = $"Execution {executionId} not found" });
                }

                // Get branches for this execution
                var branches = await _branchManagementService.GetBranchesForExecutionAsync(executionId);

                var response = new ExecutionStatusResponse
                {
                    ExecutionId = executionId,
                    Status = executionContext.Status,
                    StartTimestamp = executionContext.StartTimestamp,
                    EndTimestamp = executionContext.EndTimestamp,
                    Branches = branches
                        .Select(b => new BranchStatusInfo
                        {
                            BranchId = b.BranchId,
                            Status = b.Status.ToString(),
                            CompletedSteps = b.CompletedSteps.Count,
                            PendingSteps = b.PendingSteps.Count
                        })
                        .ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for execution {ExecutionId}", executionId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "FlowExecutionController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "GetExecutionStatus", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext { ExecutionId = executionId });

                return NotFound(new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }

        /// <summary>
        /// Cancels a flow execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The result of the operation.</returns>
        [HttpPost("{executionId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelExecution(string executionId)
        {
            try
            {
                _logger.LogInformation("Received request to cancel execution {ExecutionId}", executionId);

                // Start telemetry for this operation
                using var activity = _telemetryService.CreateStepActivity("API", "CancelExecution", "CONTROLLER", "FlowExecutionController");

                // Get execution context from repository
                var executionContext = await _executionRepository.GetExecutionContextAsync(executionId);
                if (executionContext == null)
                {
                    return NotFound(new { error = $"Execution {executionId} not found" });
                }

                // Update execution status
                executionContext.Status = ExecutionStatus.CANCELLED;
                executionContext.EndTimestamp = DateTime.UtcNow;
                await _executionRepository.UpdateExecutionContextAsync(executionContext);

                // Update all active branches to cancelled
                var branches = await _branchManagementService.GetBranchesForExecutionAsync(executionId);
                foreach (var branch in branches.Where(b => b.Status == BranchStatus.NEW || b.Status == BranchStatus.IN_PROGRESS))
                {
                    await _branchManagementService.UpdateBranchStatusAsync(executionId, branch.BranchId, BranchStatus.CANCELLED);
                }

                // Record execution completion in telemetry
                _telemetryService.RecordFlowExecutionEnd(executionId, executionContext.FlowId, ExecutionStatus.CANCELLED);

                return Ok(new { message = $"Execution {executionId} cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);

                // Record error in telemetry
                var error = new ExecutionError
                {
                    ErrorCode = "API_ERROR",
                    Timestamp = DateTime.UtcNow,
                    ServiceId = "FlowExecutionController"
                };

                // Determine recovery action
                var recoveryAction = _errorHandlingService.HandleError("API", "CancelExecution", error, new FlowOrchestrator.Abstractions.Common.ExecutionContext { ExecutionId = executionId });

                return NotFound(new { error = ex.Message, recoveryAction = recoveryAction.ToString() });
            }
        }
    }
}
