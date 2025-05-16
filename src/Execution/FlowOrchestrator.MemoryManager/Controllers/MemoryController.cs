using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlowOrchestrator.MemoryManager.Controllers
{
    /// <summary>
    /// Controller for memory management operations.
    /// </summary>
    [ApiController]
    [Route("api/memory")]
    public class MemoryController : ControllerBase
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MemoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryController"/> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="logger">The logger.</param>
        public MemoryController(
            IMemoryManager memoryManager,
            IMessageBus messageBus,
            ILogger<MemoryController> logger)
        {
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Allocates memory.
        /// </summary>
        /// <param name="request">The memory allocation request.</param>
        /// <returns>The memory allocation result.</returns>
        [HttpPost("allocate")]
        public async Task<ActionResult<FlowOrchestrator.Abstractions.Messaging.Messages.MemoryAllocationResult>> AllocateMemory([FromBody] Models.MemoryAllocationRequest request)
        {
            try
            {
                _logger.LogInformation("Received request to allocate memory for execution {ExecutionId}, flow {FlowId}, step type {StepType}, branch {BranchPath}, step {StepId}, data type {DataType}",
                    request.ExecutionId, request.FlowId, request.StepType, request.BranchPath, request.StepId, request.DataType);

                var result = await _memoryManager.AllocateMemoryAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating memory");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Allocates memory using the message bus.
        /// </summary>
        /// <param name="command">The memory allocation command.</param>
        /// <returns>A success message.</returns>
        [HttpPost("allocate-async")]
        public async Task<ActionResult<string>> AllocateMemoryAsync([FromBody] MemoryAllocationCommand command)
        {
            try
            {
                _logger.LogInformation("Received request to allocate memory asynchronously: {Command}", command);

                // Publish the command to the message bus
                await _messageBus.PublishAsync(command);

                return Ok($"Memory allocation command {command.CommandId} has been queued for processing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing memory allocation command");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deallocates memory.
        /// </summary>
        /// <param name="memoryAddress">The memory address to deallocate.</param>
        /// <returns>A result indicating success or failure.</returns>
        [HttpDelete("deallocate")]
        public async Task<ActionResult<bool>> DeallocateMemory([FromQuery] string memoryAddress)
        {
            try
            {
                _logger.LogInformation("Received request to deallocate memory for address: {Address}", memoryAddress);

                var result = await _memoryManager.DeallocateMemoryAsync(memoryAddress);

                if (result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new { error = "Failed to deallocate memory" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deallocating memory");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Checks if a memory address exists.
        /// </summary>
        /// <param name="memoryAddress">The memory address to check.</param>
        /// <returns>A result indicating whether the memory address exists.</returns>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> ExistsMemory([FromQuery] string memoryAddress)
        {
            try
            {
                _logger.LogInformation("Received request to check if memory address exists: {Address}", memoryAddress);

                var result = await _memoryManager.ExistsAsync(memoryAddress);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking memory existence");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets memory statistics.
        /// </summary>
        /// <returns>Memory statistics.</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, object>>> GetMemoryStatistics()
        {
            try
            {
                _logger.LogInformation("Received request to get memory statistics");

                var result = await _memoryManager.GetMemoryStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memory statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cleans up memory for an execution.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <returns>The number of memory addresses cleaned up.</returns>
        [HttpDelete("cleanup/execution/{executionId}")]
        public async Task<ActionResult<int>> CleanupExecutionMemory(string executionId)
        {
            try
            {
                _logger.LogInformation("Received request to clean up memory for execution: {ExecutionId}", executionId);

                var result = await _memoryManager.CleanupExecutionMemoryAsync(executionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up execution memory");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cleans up memory for a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The number of memory addresses cleaned up.</returns>
        [HttpDelete("cleanup/branch/{executionId}/{branchId}")]
        public async Task<ActionResult<int>> CleanupBranchMemory(string executionId, string branchId)
        {
            try
            {
                _logger.LogInformation("Received request to clean up memory for branch: {BranchId} of execution: {ExecutionId}", branchId, executionId);

                var result = await _memoryManager.CleanupBranchMemoryAsync(executionId, branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up branch memory");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Validates access to a memory address.
        /// </summary>
        /// <param name="memoryAddress">The memory address to validate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A result indicating whether access is allowed.</returns>
        [HttpPost("validate-access")]
        public async Task<ActionResult<bool>> ValidateAccess([FromQuery] string memoryAddress, [FromBody] FlowOrchestrator.Abstractions.Common.ExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Received request to validate access to memory address: {Address} for context: {Context}", memoryAddress, context);

                var result = await _memoryManager.ValidateAccessAsync(memoryAddress, context);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating memory access");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
