using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Orchestrator.Messaging.Consumers
{
    /// <summary>
    /// Consumer for TriggerFlowCommand messages.
    /// </summary>
    public class TriggerFlowCommandConsumer : IMessageConsumer<TriggerFlowCommand>
    {
        private readonly ILogger<TriggerFlowCommandConsumer> _logger;
        private readonly OrchestratorService _orchestratorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFlowCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="orchestratorService">The orchestrator service.</param>
        public TriggerFlowCommandConsumer(
            ILogger<TriggerFlowCommandConsumer> logger,
            OrchestratorService orchestratorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orchestratorService = orchestratorService ?? throw new ArgumentNullException(nameof(orchestratorService));
        }

        /// <summary>
        /// Consumes a TriggerFlowCommand message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<TriggerFlowCommand> context)
        {
            try
            {
                _logger.LogInformation("Received TriggerFlowCommand for flow {FlowId}", context.Message.FlowEntityId);

                // In a real implementation, we would retrieve the flow entity from a repository
                // and start the flow execution
                // For now, we'll just log the message

                _logger.LogInformation("TriggerFlowCommand processed for flow {FlowId}", context.Message.FlowEntityId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TriggerFlowCommand for flow {FlowId}", context.Message.FlowEntityId);
                throw;
            }
        }
    }
}
