using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Integration.Importers;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Management.Scheduling.Messaging.Results;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlowOrchestrator.Management.Scheduling.Services
{
    /// <summary>
    /// Service for executing flows.
    /// </summary>
    public class FlowExecutionService
    {
        private readonly ILogger<FlowExecutionService> _logger;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowExecutionService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBus">The message bus.</param>
        public FlowExecutionService(
            ILogger<FlowExecutionService> logger,
            IMessageBus messageBus)
        {
            _logger = logger;
            _messageBus = messageBus;
        }

        /// <summary>
        /// Executes a flow.
        /// </summary>
        /// <param name="command">The trigger flow command.</param>
        /// <returns>The flow execution result.</returns>
        public async Task<FlowExecutionResult> ExecuteFlowAsync(TriggerFlowCommand command)
        {
            try
            {
                _logger.LogInformation("Executing flow {ScheduledFlowEntityId} with task scheduler {TaskSchedulerEntityId}",
                    command.ScheduledFlowEntityId, command.TaskSchedulerEntityId);

                // In a real implementation, we would retrieve the flow entity, source assignment, and destination assignment from a repository
                // Then we would create and publish an ImportCommand to start the flow execution
                // For now, we'll just simulate the flow execution

                // Create a new execution context
                var executionContext = new ExecutionContext
                {
                    ExecutionId = Guid.NewGuid().ToString(),
                    FlowId = command.FlowEntityId,
                    StartTimestamp = DateTime.UtcNow,
                    Status = ExecutionStatus.RUNNING,
                    Parameters = new Dictionary<string, object>(command.FlowParameters.ToDictionary(kv => kv.Key, kv => (object)kv.Value))
                };

                // Simulate some work
                await Task.Delay(100);

                // Update the execution context
                executionContext.EndTimestamp = DateTime.UtcNow;
                executionContext.Status = ExecutionStatus.COMPLETED;

                // Store results in metadata since ExecutionContext doesn't have a Results property
                executionContext.Metadata["ProcessedRecords"] = 100;
                executionContext.Metadata["SuccessCount"] = 95;
                executionContext.Metadata["ErrorCount"] = 5;
                executionContext.Metadata["ExecutionTimeMs"] = (executionContext.EndTimestamp.Value - executionContext.StartTimestamp).TotalMilliseconds;

                _logger.LogInformation("Flow {ScheduledFlowEntityId} executed successfully", command.ScheduledFlowEntityId);

                // Return the result
                return new FlowExecutionResult(
                    command,
                    true,
                    ExecutionStatus.COMPLETED,
                    executionContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing flow {ScheduledFlowEntityId}", command.ScheduledFlowEntityId);

                var errorDetails = new Dictionary<string, object>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message },
                    { "StackTrace", ex.StackTrace ?? string.Empty }
                };

                return new FlowExecutionResult(
                    command,
                    $"Error executing flow: {ex.Message}",
                    errorDetails,
                    command.Context);
            }
        }

        /// <summary>
        /// Publishes a flow execution result.
        /// </summary>
        /// <param name="result">The flow execution result.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PublishFlowExecutionResultAsync(FlowExecutionResult result)
        {
            try
            {
                _logger.LogInformation("Publishing flow execution result for flow {ScheduledFlowEntityId}",
                    result.Command.ScheduledFlowEntityId);

                await _messageBus.PublishAsync(result);

                _logger.LogInformation("Flow execution result published successfully for flow {ScheduledFlowEntityId}",
                    result.Command.ScheduledFlowEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing flow execution result for flow {ScheduledFlowEntityId}",
                    result.Command.ScheduledFlowEntityId);
                throw;
            }
        }
    }
}
