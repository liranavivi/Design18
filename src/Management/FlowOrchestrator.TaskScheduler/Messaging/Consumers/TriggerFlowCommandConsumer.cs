using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Management.Scheduling.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Management.Scheduling.Messaging.Consumers
{
    /// <summary>
    /// Consumer for trigger flow commands.
    /// </summary>
    public class TriggerFlowCommandConsumer : IConsumer<TriggerFlowCommand>
    {
        private readonly ILogger<TriggerFlowCommandConsumer> _logger;
        private readonly TaskSchedulerService _taskSchedulerService;
        private readonly FlowExecutionService _flowExecutionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFlowCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="taskSchedulerService">The task scheduler service.</param>
        /// <param name="flowExecutionService">The flow execution service.</param>
        public TriggerFlowCommandConsumer(
            ILogger<TriggerFlowCommandConsumer> logger,
            TaskSchedulerService taskSchedulerService,
            FlowExecutionService flowExecutionService)
        {
            _logger = logger;
            _taskSchedulerService = taskSchedulerService;
            _flowExecutionService = flowExecutionService;
        }

        /// <summary>
        /// Consumes the trigger flow command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<TriggerFlowCommand> context)
        {
            var command = context.Message;

            try
            {
                _logger.LogInformation("Received trigger flow command for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);

                // Trigger the flow
                var result = await _taskSchedulerService.TriggerFlowAsync(command);

                // If the flow was triggered successfully, execute it
                if (result.Success)
                {
                    // Execute the flow
                    var executionResult = await _flowExecutionService.ExecuteFlowAsync(command);

                    // Publish the execution result
                    await _flowExecutionService.PublishFlowExecutionResultAsync(executionResult);
                }
                else
                {
                    // Publish the error result
                    await _flowExecutionService.PublishFlowExecutionResultAsync(result);
                }

                _logger.LogInformation("Trigger flow command processed successfully for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trigger flow command for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);
                throw;
            }
        }
    }
}
