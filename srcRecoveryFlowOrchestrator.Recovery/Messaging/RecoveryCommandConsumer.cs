using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Recovery.Services;
using FlowOrchestrator.Recovery.Strategies;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Messaging
{
    /// <summary>
    /// Consumer for recovery commands.
    /// </summary>
    public class RecoveryCommandConsumer : IMessageConsumer<RecoveryCommand>
    {
        private readonly ILogger<RecoveryCommandConsumer> _logger;
        private readonly RecoveryService _recoveryService;
        private readonly IEnumerable<IRecoveryStrategy> _strategies;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryCommandConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="recoveryService">The recovery service.</param>
        /// <param name="strategies">The recovery strategies.</param>
        /// <param name="messageBus">The message bus.</param>
        public RecoveryCommandConsumer(
            ILogger<RecoveryCommandConsumer> logger,
            RecoveryService recoveryService,
            IEnumerable<IRecoveryStrategy> strategies,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recoveryService = recoveryService ?? throw new ArgumentNullException(nameof(recoveryService));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Consumes a recovery command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<RecoveryCommand> context)
        {
            var command = context.Message;

            _logger.LogInformation("Consuming recovery command {CommandId} for strategy {StrategyName}",
                command.CommandId, command.StrategyName);

            try
            {
                // Find the strategy
                var strategy = _strategies.FirstOrDefault(s => s.Name == command.StrategyName);
                if (strategy == null)
                {
                    _logger.LogWarning("Strategy {StrategyName} not found", command.StrategyName);
                    
                    await PublishFailedResult(command, $"Strategy {command.StrategyName} not found");
                    return;
                }

                // Initialize the strategy with the parameters
                strategy.Initialize(command.StrategyParameters);

                // Create a recovery context
                var recoveryContext = new Models.RecoveryContext
                {
                    RecoveryId = command.RecoveryId,
                    ErrorContext = command.ErrorContext,
                    ExecutionContext = command.ExecutionContext,
                    StrategyName = command.StrategyName,
                    Parameters = command.StrategyParameters
                };

                // Execute the strategy
                var recoveryResult = strategy.Recover(recoveryContext);

                // Publish the result
                await PublishResult(command, recoveryResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing recovery command {CommandId}", command.CommandId);
                
                await PublishFailedResult(command, $"Error executing recovery command: {ex.Message}");
            }
        }

        private async Task PublishResult(RecoveryCommand command, Models.RecoveryResult recoveryResult)
        {
            var result = new RecoveryCommandResult
            {
                ResultId = Guid.NewGuid().ToString(),
                Success = recoveryResult.Success,
                RecoveryResult = recoveryResult
            };

            await _messageBus.PublishAsync(result);

            _logger.LogInformation("Published recovery command result {ResultId} with success {Success}",
                result.ResultId, result.Success);
        }

        private async Task PublishFailedResult(RecoveryCommand command, string errorMessage)
        {
            var result = new RecoveryCommandResult
            {
                ResultId = Guid.NewGuid().ToString(),
                Success = false,
                Error = new Abstractions.Common.ExecutionError
                {
                    ErrorCode = "RECOVERY_ERROR",
                    ErrorMessage = errorMessage,
                    ErrorDetails = new Dictionary<string, object>
                    {
                        ["CommandId"] = command.CommandId,
                        ["StrategyName"] = command.StrategyName
                    }
                }
            };

            await _messageBus.PublishAsync(result);

            _logger.LogInformation("Published failed recovery command result {ResultId}: {ErrorMessage}",
                result.ResultId, errorMessage);
        }
    }
}
