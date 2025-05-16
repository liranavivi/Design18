using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.BranchController.Messaging.Commands;
using FlowOrchestrator.BranchController.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.BranchController.Messaging.Consumers
{
    /// <summary>
    /// Consumer for the create branch command.
    /// </summary>
    public class CreateBranchCommandConsumer : IMessageConsumer<CreateBranchCommand>
    {
        private readonly ILogger<CreateBranchCommandConsumer> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly TelemetryService _telemetryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBranchCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        /// <param name="telemetryService">The telemetry service.</param>
        public CreateBranchCommandConsumer(
            ILogger<CreateBranchCommandConsumer> logger,
            BranchContextService branchContextService,
            TelemetryService telemetryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        /// <summary>
        /// Consumes the create branch command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<CreateBranchCommand> context)
        {
            var command = context.Message;
            
            _logger.LogInformation("Received create branch command for branch {BranchId} of execution {ExecutionId}",
                command.BranchId, command.ExecutionId);

            using var activity = _telemetryService.CreateStepActivity(
                "COMMAND", "CreateBranch", "CONSUMER", "CreateBranchCommandConsumer");

            try
            {
                if (command.Context == null)
                {
                    _logger.LogWarning("Create branch command has no execution context");
                    return;
                }

                // Create the branch
                var branchContext = await _branchContextService.CreateBranchAsync(
                    command.Context, command.BranchId, command.ParentBranchId);

                // Update branch properties from command
                branchContext.Priority = command.Priority;
                branchContext.TimeoutMs = command.TimeoutMs;
                branchContext.Configuration = new Dictionary<string, object>(command.Configuration);
                branchContext.Parameters = new Dictionary<string, object>(command.Parameters);

                _logger.LogInformation("Created branch {BranchId} for execution {ExecutionId}",
                    command.BranchId, command.ExecutionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch {BranchId} for execution {ExecutionId}",
                    command.BranchId, command.ExecutionId);
            }
        }
    }
}
