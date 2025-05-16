using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Messaging.Consumers
{
    /// <summary>
    /// Consumer for ProcessCommandResult messages.
    /// </summary>
    public class ProcessCommandResultConsumer : IMessageConsumer<ProcessCommandResult>
    {
        private readonly ILogger<ProcessCommandResultConsumer> _logger;
        private readonly OrchestratorService _orchestratorService;
        private readonly BranchManagementService _branchManagementService;
        private readonly MemoryAddressService _memoryAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessCommandResultConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="orchestratorService">The orchestrator service.</param>
        /// <param name="branchManagementService">The branch management service.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        public ProcessCommandResultConsumer(
            ILogger<ProcessCommandResultConsumer> logger,
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
        /// Consumes a ProcessCommandResult message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<ProcessCommandResult> context)
        {
            try
            {
                var result = context.Message;
                _logger.LogInformation("Received ProcessCommandResult for command {CommandId}, processor {ProcessorId}",
                    result.Command.CommandId, result.Command.ProcessorServiceId);

                // Parse the memory address to get execution and branch information
                var outputAddressComponents = _memoryAddressService.ParseMemoryAddress(result.Command.OutputLocation);
                
                if (outputAddressComponents.TryGetValue("executionId", out var executionId) &&
                    outputAddressComponents.TryGetValue("branchPath", out var branchId) &&
                    outputAddressComponents.TryGetValue("stepId", out var processorId))
                {
                    // Update branch status with completed step
                    _branchManagementService.AddCompletedStep(executionId, branchId, processorId);
                    
                    // In a real implementation, we would:
                    // 1. Check if there are next steps in the flow for this branch
                    // 2. If yes, send commands to the next processors
                    // 3. If no, check if this branch is complete and update its status
                    // 4. Check if all branches are complete and trigger merge operations if needed
                    
                    _logger.LogInformation("Processed result for processor {ProcessorId} in branch {BranchId} of execution {ExecutionId}",
                        processorId, branchId, executionId);
                }
                else
                {
                    _logger.LogWarning("Could not parse memory address {Address}", result.Command.OutputLocation);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ProcessCommandResult");
                throw;
            }
        }
    }
}
