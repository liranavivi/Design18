using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FlowOrchestrator.MemoryManager.Messaging
{
    /// <summary>
    /// Consumer for memory allocation commands.
    /// </summary>
    public class MemoryAllocationCommandConsumer : IMessageConsumer<MemoryAllocationCommand>
    {
        private readonly ILogger<MemoryAllocationCommandConsumer> _logger;
        private readonly IMemoryManagerService _memoryManager;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAllocationCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="memoryManager">The memory manager service.</param>
        /// <param name="messageBus">The message bus.</param>
        public MemoryAllocationCommandConsumer(
            ILogger<MemoryAllocationCommandConsumer> logger,
            IMemoryManagerService memoryManager,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Consumes a memory allocation command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<MemoryAllocationCommand> context)
        {
            if (context == null || context.Message == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var command = context.Message;
            _logger.LogInformation("Received memory allocation command: {CommandId}", command.CommandId);

            var result = new MemoryAllocationResult
            {
                CommandId = command.CommandId,
                ExecutionId = command.ExecutionId,
                FlowId = command.FlowId,
                StepId = command.StepId
            };

            try
            {
                // Allocate memory
                var memoryAddress = await _memoryManager.AllocateMemoryAsync(
                    command.Size,
                    command.ExecutionId,
                    command.FlowId,
                    command.StepId,
                    command.MemoryType,
                    TimeSpan.FromSeconds(command.TimeToLiveSeconds));

                // Set success result
                result.Success = true;
                result.MemoryAddress = memoryAddress;

                _logger.LogInformation("Memory allocation successful: {CommandId}, Address: {MemoryAddress}", 
                    command.CommandId, memoryAddress);
            }
            catch (Exception ex)
            {
                // Set failure result
                result.Success = false;
                result.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Memory allocation failed: {CommandId}", command.CommandId);
            }

            // Publish the result
            await _messageBus.PublishAsync(result);
        }
    }
}
