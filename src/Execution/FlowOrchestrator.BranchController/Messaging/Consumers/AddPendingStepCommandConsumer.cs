using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.BranchController.Messaging.Commands;
using FlowOrchestrator.BranchController.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.BranchController.Messaging.Consumers
{
    /// <summary>
    /// Consumer for the add pending step command.
    /// </summary>
    public class AddPendingStepCommandConsumer : IMessageConsumer<AddPendingStepCommand>
    {
        private readonly ILogger<AddPendingStepCommandConsumer> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly TelemetryService _telemetryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPendingStepCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public AddPendingStepCommandConsumer(
            ILogger<AddPendingStepCommandConsumer> logger,
            BranchContextService branchContextService,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        /// <summary>
        /// Consumes the add pending step command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<AddPendingStepCommand> context)
        {
            var command = context.Message;
            
            _logger.LogInformation("Received add pending step command for step {StepId} in branch {BranchId} of execution {ExecutionId}",
                command.StepId, command.BranchId, command.ExecutionId);

            using var activity = _telemetryService.CreateStepActivity(
                "COMMAND", "AddPendingStep", "CONSUMER", "AddPendingStepCommandConsumer");

            try
            {
                // Add the pending step
                var result = _branchContextService.AddPendingStep(
                    command.ExecutionId, command.BranchId, command.StepId);

                if (result)
                {
                    _logger.LogInformation("Added pending step {StepId} to branch {BranchId}",
                        command.StepId, command.BranchId);
                }
                else
                {
                    _logger.LogWarning("Failed to add pending step {StepId} to branch {BranchId}",
                        command.StepId, command.BranchId);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding pending step {StepId} to branch {BranchId}",
                    command.StepId, command.BranchId);
            }
        }
    }
}
