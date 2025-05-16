using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Messaging.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Messaging.Consumers
{
    /// <summary>
    /// Consumer for DeallocateMemoryCommand messages.
    /// </summary>
    public class DeallocateMemoryCommandConsumer : IConsumer<DeallocateMemoryCommand>
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<DeallocateMemoryCommandConsumer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeallocateMemoryCommandConsumer"/> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="logger">The logger.</param>
        public DeallocateMemoryCommandConsumer(
            IMemoryManager memoryManager,
            IMessageBus messageBus,
            ILogger<DeallocateMemoryCommandConsumer> logger)
        {
            _memoryManager = memoryManager;
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// Consumes the DeallocateMemoryCommand message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<DeallocateMemoryCommand> context)
        {
            var command = context.Message;
            _logger.LogInformation("Received DeallocateMemoryCommand: {CommandId}", command.CommandId);

            try
            {
                // Validate access if context is provided
                if (command.Context != null)
                {
                    var hasAccess = await _memoryManager.ValidateAccessAsync(command.MemoryAddress, command.Context);
                    if (!hasAccess)
                    {
                        _logger.LogWarning("Access denied to memory address: {Address} for context: {Context}", command.MemoryAddress, command.Context);
                        
                        // Publish error result
                        var errorResult = new DeallocateMemoryCommandResult
                        {
                            CommandId = command.CommandId,
                            Success = false,
                            MemoryAddress = command.MemoryAddress,
                            ErrorMessage = "Access denied"
                        };

                        await _messageBus.PublishAsync(errorResult);
                        return;
                    }
                }

                // Deallocate memory
                var success = await _memoryManager.DeallocateMemoryAsync(command.MemoryAddress);

                // Create command result
                var commandResult = new DeallocateMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = success,
                    MemoryAddress = command.MemoryAddress,
                    ErrorMessage = success ? null : "Failed to deallocate memory"
                };

                // Publish result
                await _messageBus.PublishAsync(commandResult);
                _logger.LogInformation("Published DeallocateMemoryCommandResult: {ResultId}", commandResult.ResultId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DeallocateMemoryCommand: {CommandId}", command.CommandId);

                // Publish error result
                var errorResult = new DeallocateMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = false,
                    MemoryAddress = command.MemoryAddress,
                    ErrorMessage = ex.Message
                };

                await _messageBus.PublishAsync(errorResult);
                _logger.LogInformation("Published error DeallocateMemoryCommandResult: {ResultId}", errorResult.ResultId);
            }
        }
    }
}
