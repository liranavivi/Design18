using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Messaging.Commands;
using FlowOrchestrator.MemoryManager.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.MemoryManager.Messaging.Consumers
{
    /// <summary>
    /// Consumer for AllocateMemoryCommand messages.
    /// </summary>
    public class AllocateMemoryCommandConsumer : IConsumer<AllocateMemoryCommand>
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<AllocateMemoryCommandConsumer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocateMemoryCommandConsumer"/> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="messageBus">The message bus.</param>
        /// <param name="logger">The logger.</param>
        public AllocateMemoryCommandConsumer(
            IMemoryManager memoryManager,
            IMessageBus messageBus,
            ILogger<AllocateMemoryCommandConsumer> logger)
        {
            _memoryManager = memoryManager;
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// Consumes the AllocateMemoryCommand message.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<AllocateMemoryCommand> context)
        {
            var command = context.Message;
            _logger.LogInformation("Received AllocateMemoryCommand: {CommandId}", command.CommandId);

            try
            {
                // Create allocation request
                var request = new MemoryAllocationRequest
                {
                    ExecutionId = command.ExecutionId,
                    FlowId = command.FlowId,
                    StepType = command.StepType,
                    BranchPath = command.BranchPath,
                    StepId = command.StepId,
                    DataType = command.DataType,
                    AdditionalInfo = command.AdditionalInfo,
                    Context = command.Context,
                    TimeToLiveSeconds = command.TimeToLiveSeconds,
                    EstimatedSizeBytes = command.EstimatedSizeBytes
                };

                // Allocate memory
                var result = await _memoryManager.AllocateMemoryAsync(request);

                // Create command result
                var commandResult = new AllocateMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = result.Success,
                    MemoryAddress = result.MemoryAddress,
                    ErrorMessage = result.ErrorMessage,
                    AllocationTimestamp = DateTime.UtcNow,
                    ExpirationTimestamp = request.TimeToLiveSeconds.HasValue ?
                        DateTime.UtcNow.AddSeconds(request.TimeToLiveSeconds.Value) : null
                };

                // Publish result
                await _messageBus.PublishAsync(commandResult);
                _logger.LogInformation("Published AllocateMemoryCommandResult: {ResultId}", commandResult.ResultId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AllocateMemoryCommand: {CommandId}", command.CommandId);

                // Publish error result
                var errorResult = new AllocateMemoryCommandResult
                {
                    CommandId = command.CommandId,
                    Success = false,
                    ErrorMessage = ex.Message
                };

                await _messageBus.PublishAsync(errorResult);
                _logger.LogInformation("Published error AllocateMemoryCommandResult: {ResultId}", errorResult.ResultId);
            }
        }
    }
}
