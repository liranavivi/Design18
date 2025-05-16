using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.BranchController.Messaging.Commands;
using FlowOrchestrator.BranchController.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.BranchController.Messaging.Consumers
{
    /// <summary>
    /// Consumer for the add completed step command.
    /// </summary>
    public class AddCompletedStepCommandConsumer : IMessageConsumer<AddCompletedStepCommand>
    {
        private readonly ILogger<AddCompletedStepCommandConsumer> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly BranchCompletionService _branchCompletionService;
        private readonly TelemetryService _telemetryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddCompletedStepCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        /// <param name="branchCompletionService">The branch completion service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public AddCompletedStepCommandConsumer(
            ILogger<AddCompletedStepCommandConsumer> logger,
            BranchContextService branchContextService,
            BranchCompletionService branchCompletionService,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _branchCompletionService = branchCompletionService ?? throw new ArgumentNullException(nameof(branchCompletionService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        /// <summary>
        /// Consumes the add completed step command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<AddCompletedStepCommand> context)
        {
            var command = context.Message;
            
            _logger.LogInformation("Received add completed step command for step {StepId} in branch {BranchId} of execution {ExecutionId}",
                command.StepId, command.BranchId, command.ExecutionId);

            using var activity = _telemetryService.CreateStepActivity(
                "COMMAND", "AddCompletedStep", "CONSUMER", "AddCompletedStepCommandConsumer");

            try
            {
                // Add the completed step
                var result = _branchContextService.AddCompletedStep(
                    command.ExecutionId, command.BranchId, command.StepId);

                if (result)
                {
                    _logger.LogInformation("Added completed step {StepId} to branch {BranchId}",
                        command.StepId, command.BranchId);

                    // Check if the branch is complete
                    if (_branchCompletionService.IsBranchComplete(command.ExecutionId, command.BranchId))
                    {
                        _logger.LogInformation("Branch {BranchId} is complete, completing branch",
                            command.BranchId);

                        // Complete the branch
                        await _branchCompletionService.CompleteBranchAsync(
                            command.ExecutionId, command.BranchId);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to add completed step {StepId} to branch {BranchId}",
                        command.StepId, command.BranchId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding completed step {StepId} to branch {BranchId}",
                    command.StepId, command.BranchId);
            }
        }
    }
}
