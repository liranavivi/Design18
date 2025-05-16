using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Messaging.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Messaging.Consumers
{
    /// <summary>
    /// Consumer for CleanupExecutionMemoryCommand messages.
    /// </summary>
    public class CleanupExecutionMemoryCommandConsumer : IConsumer<CleanupExecutionMemoryCommand>
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<CleanupExecutionMemoryCommandConsumer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupExecutionMemoryCommandConsumer"/> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="logger">The logger.</param>
        public CleanupExecutionMemoryCommandConsumer(
            IMemoryManager memoryManager,
            IMessageBus messageBus,
            ILogger<CleanupExecutionMemoryCommandConsumer> logger)
        {
            _memoryManager = memoryManager;
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// Consumes the CleanupExecutionMemoryCommand message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<CleanupExecutionMemoryCommand> context)
        {
            var command = context.Message;
            _logger.LogInformation("Received CleanupExecutionMemoryCommand: {CommandId} for execution: {ExecutionId}", command.CommandId, command.ExecutionId);

            try
            {
                // Clean up execution memory
                var cleanedUpCount = await _memoryManager.CleanupExecutionMemoryAsync(command.ExecutionId);

                // Create command result
                var commandResult = new CleanupExecutionMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = true,
                    ExecutionId = command.ExecutionId,
                    CleanedUpCount = cleanedUpCount
                };

                // Publish result
                await _messageBus.PublishAsync(commandResult);
                _logger.LogInformation("Published CleanupExecutionMemoryCommandResult: {ResultId} for execution: {ExecutionId}, cleaned up: {CleanedUpCount}", commandResult.ResultId, command.ExecutionId, cleanedUpCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CleanupExecutionMemoryCommand: {CommandId} for execution: {ExecutionId}", command.CommandId, command.ExecutionId);

                // Publish error result
                var errorResult = new CleanupExecutionMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = false,
                    ExecutionId = command.ExecutionId,
                    CleanedUpCount = 0,
                    ErrorMessage = ex.Message
                };

                await _messageBus.PublishAsync(errorResult);
                _logger.LogInformation("Published error CleanupExecutionMemoryCommandResult: {ResultId} for execution: {ExecutionId}", errorResult.ResultId, command.ExecutionId);
            }
        }
    }
}
