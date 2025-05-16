using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.BranchController.Messaging.Commands;
using FlowOrchestrator.BranchController.Models;
using FlowOrchestrator.BranchController.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.BranchController.Messaging.Consumers
{
    /// <summary>
    /// Consumer for the update branch status command.
    /// </summary>
    public class UpdateBranchStatusCommandConsumer : IMessageConsumer<UpdateBranchStatusCommand>
    {
        private readonly ILogger<UpdateBranchStatusCommandConsumer> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly TelemetryService _telemetryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBranchStatusCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public UpdateBranchStatusCommandConsumer(
            ILogger<UpdateBranchStatusCommandConsumer> logger,
            BranchContextService branchContextService,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        /// <summary>
        /// Consumes the update branch status command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<UpdateBranchStatusCommand> context)
        {
            var command = context.Message;
            
            _logger.LogInformation("Received update branch status command for branch {BranchId} of execution {ExecutionId} to status {Status}",
                command.BranchId, command.ExecutionId, command.Status);

            using var activity = _telemetryService.CreateStepActivity(
                "COMMAND", "UpdateBranchStatus", "CONSUMER", "UpdateBranchStatusCommandConsumer");

            try
            {
                if (!Enum.TryParse<BranchStatus>(command.Status, true, out var status))
                {
                    _logger.LogWarning("Invalid branch status: {Status}", command.Status);
                    return;
                }

                // Update the branch status
                var result = await _branchContextService.UpdateBranchStatusAsync(
                    command.ExecutionId, command.BranchId, status);

                if (result)
                {
                    _logger.LogInformation("Updated branch {BranchId} status to {Status}",
                        command.BranchId, command.Status);
                }
                else
                {
                    _logger.LogWarning("Failed to update branch {BranchId} status to {Status}",
                        command.BranchId, command.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch {BranchId} status to {Status}",
                    command.BranchId, command.Status);
            }
        }
    }
}
