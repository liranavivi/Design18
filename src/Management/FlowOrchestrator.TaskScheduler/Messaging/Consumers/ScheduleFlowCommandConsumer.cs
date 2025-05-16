using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Management.Scheduling.Messaging.Results;
using FlowOrchestrator.Management.Scheduling.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Management.Scheduling.Messaging.Consumers
{
    /// <summary>
    /// Consumer for schedule flow commands.
    /// </summary>
    public class ScheduleFlowCommandConsumer : IConsumer<ScheduleFlowCommand>
    {
        private readonly ILogger<ScheduleFlowCommandConsumer> _logger;
        private readonly TaskSchedulerService _taskSchedulerService;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFlowCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="taskSchedulerService">The task scheduler service.</param>
        /// <param name="messageBus">The message bus.</param>
        public ScheduleFlowCommandConsumer(
            ILogger<ScheduleFlowCommandConsumer> logger,
            TaskSchedulerService taskSchedulerService,
            IMessageBus messageBus)
        {
            _logger = logger;
            _taskSchedulerService = taskSchedulerService;
            _messageBus = messageBus;
        }

        /// <summary>
        /// Consumes the schedule flow command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<ScheduleFlowCommand> context)
        {
            var command = context.Message;

            try
            {
                _logger.LogInformation("Received schedule flow command for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);

                // Schedule the flow
                var result = await _taskSchedulerService.ScheduleFlowAsync(command);

                // Publish the result
                await _messageBus.PublishAsync(result);

                _logger.LogInformation("Schedule flow command processed successfully for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing schedule flow command for flow {ScheduledFlowEntityId}",
                    command.ScheduledFlowEntityId);

                // Publish an error result
                var errorDetails = new Dictionary<string, object>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message },
                    { "StackTrace", ex.StackTrace ?? string.Empty }
                };

                var errorResult = new FlowSchedulingResult(
                    command,
                    $"Error scheduling flow: {ex.Message}",
                    errorDetails);

                await _messageBus.PublishAsync(errorResult);

                throw;
            }
        }
    }
}
