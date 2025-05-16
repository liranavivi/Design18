using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Messaging.Consumers
{
    /// <summary>
    /// Consumer for ImportCommandResult messages.
    /// </summary>
    public class ImportCommandResultConsumer : IMessageConsumer<ImportCommandResult>
    {
        private readonly ILogger<ImportCommandResultConsumer> _logger;
        private readonly OrchestratorService _orchestratorService;
        private readonly BranchManagementService _branchManagementService;
        private readonly MemoryAddressService _memoryAddressService;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommandResultConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="orchestratorService">The orchestrator service.</param>
        /// <param name="branchManagementService">The branch management service.</param>
        /// <param name="memoryAddressService">The memory address service.</param>
        /// <param name="messageBus">The message bus.</param>
        public ImportCommandResultConsumer(
            ILogger<ImportCommandResultConsumer> logger,
            OrchestratorService orchestratorService,
            BranchManagementService branchManagementService,
            MemoryAddressService memoryAddressService,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orchestratorService = orchestratorService ?? throw new ArgumentNullException(nameof(orchestratorService));
            _branchManagementService = branchManagementService ?? throw new ArgumentNullException(nameof(branchManagementService));
            _memoryAddressService = memoryAddressService ?? throw new ArgumentNullException(nameof(memoryAddressService));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Consumes an ImportCommandResult message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<ImportCommandResult> context)
        {
            try
            {
                var result = context.Message;
                _logger.LogInformation("Received ImportCommandResult for command {CommandId}, importer {ImporterId}",
                    result.Command.CommandId, result.Command.ImporterServiceId);

                // Parse the memory address to get execution information
                var outputAddressComponents = _memoryAddressService.ParseMemoryAddress(result.Command.OutputLocation);
                
                if (outputAddressComponents.TryGetValue("executionId", out var executionId) &&
                    outputAddressComponents.TryGetValue("branchPath", out var branchId) &&
                    outputAddressComponents.TryGetValue("stepId", out var importerId))
                {
                    // Update branch status with completed step
                    _branchManagementService.AddCompletedStep(executionId, branchId, importerId);
                    
                    // In a real implementation, we would:
                    // 1. Retrieve the flow entity to determine the next steps
                    // 2. Create process commands for the next processors in the flow
                    // 3. Send the process commands to the message bus
                    
                    _logger.LogInformation("Processed result for importer {ImporterId} in branch {BranchId} of execution {ExecutionId}",
                        importerId, branchId, executionId);

                    // Mock example of sending a process command to the next processor
                    // This would be replaced with actual flow topology logic
                    /*
                    var processCommand = new ProcessCommand
                    {
                        CommandId = Guid.NewGuid().ToString(),
                        ProcessorServiceId = "PROCESSOR-001",
                        ProcessorServiceVersion = "1.0.0",
                        InputLocation = result.Command.OutputLocation,
                        OutputLocation = _memoryAddressService.GenerateProcessorOutputAddress(result.Command.Context, "PROCESSOR-001"),
                        Context = result.Command.Context,
                        Parameters = new ProcessParameters()
                    };
                    
                    await _messageBus.PublishAsync(processCommand);
                    */
                }
                else
                {
                    _logger.LogWarning("Could not parse memory address {Address}", result.Command.OutputLocation);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ImportCommandResult");
                throw;
            }
        }
    }
}
