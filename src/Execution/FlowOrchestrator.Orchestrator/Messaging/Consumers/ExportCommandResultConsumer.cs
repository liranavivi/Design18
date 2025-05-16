using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Messaging.Consumers
{
    /// <summary>
    /// Consumer for ExportCommandResult messages.
    /// </summary>
    public class ExportCommandResultConsumer : IMessageConsumer<ExportCommandResult>
    {
        private readonly ILogger<ExportCommandResultConsumer> _logger;
        private readonly OrchestratorService _orchestratorService;
        private readonly BranchManagementService _branchManagementService;
        private readonly MemoryAddressService _memoryAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommandResultConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="orchestratorService">The orchestrator service.</param>
        /// <param name="branchManagementService">The branch management service.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        public ExportCommandResultConsumer(
            ILogger<ExportCommandResultConsumer> logger,
            OrchestratorService orchestratorService,
            BranchManagementService branchManagementService,
            MemoryAddressService memoryAddressService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orchestratorService = orchestratorService ?? throw new ArgumentNullException(nameof(orchestratorService));
            _branchManagementService = branchManagementService ?? throw new ArgumentNullException(nameof(branchManagementService));
            _memoryAddressService = memoryAddressService ?? throw new ArgumentNullException(nameof(memoryAddressService));
        }

        /// <summary>
        /// Consumes an ExportCommandResult message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<ExportCommandResult> context)
        {
            try
            {
                var result = context.Message;
                _logger.LogInformation("Received ExportCommandResult for command {CommandId}, exporter {ExporterId}",
                    result.Command.CommandId, result.Command.ExporterServiceId);

                // Parse the memory address to get execution information
                var inputAddressComponents = _memoryAddressService.ParseMemoryAddress(result.Command.InputLocation);
                
                if (inputAddressComponents.TryGetValue("executionId", out var executionId) &&
                    inputAddressComponents.TryGetValue("branchPath", out var branchId) &&
                    inputAddressComponents.TryGetValue("stepId", out var exporterId))
                {
                    // Update branch status with completed step
                    _branchManagementService.AddCompletedStep(executionId, branchId, exporterId);
                    
                    // Mark the branch as completed
                    _branchManagementService.UpdateBranchStatus(executionId, branchId, BranchStatus.COMPLETED);
                    
                    // Check if all branches for this execution are complete
                    var branches = _branchManagementService.GetBranchesForExecution(executionId);
                    var allBranchesComplete = branches.All(b => b.Status == BranchStatus.COMPLETED || 
                                                               b.Status == BranchStatus.FAILED || 
                                                               b.Status == BranchStatus.CANCELLED);
                    
                    if (allBranchesComplete)
                    {
                        // In a real implementation, we would:
                        // 1. Update the execution status to COMPLETED
                        // 2. Publish an execution completed event
                        // 3. Clean up resources
                        
                        _logger.LogInformation("All branches complete for execution {ExecutionId}", executionId);
                    }
                    
                    _logger.LogInformation("Processed result for exporter {ExporterId} in branch {BranchId} of execution {ExecutionId}",
                        exporterId, branchId, executionId);
                }
                else
                {
                    _logger.LogWarning("Could not parse memory address {Address}", result.Command.InputLocation);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ExportCommandResult");
                throw;
            }
        }
    }
}
