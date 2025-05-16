using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Recovery.Models;
using FlowOrchestrator.Recovery.Services;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Recovery.Messaging
{
    /// <summary>
    /// Consumer for error events.
    /// </summary>
    public class ErrorEventConsumer : IMessageConsumer<ErrorEvent>
    {
        private readonly ILogger<ErrorEventConsumer> _logger;
        private readonly RecoveryService _recoveryService;
        private readonly ErrorClassificationService _errorClassificationService;
        private readonly IMessageBus _messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventConsumer"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="recoveryService">The recovery service.</param>
        /// <param name="errorClassificationService">The error classification service.</param>
        /// <param name="messageBus">The message bus.</param>
        public ErrorEventConsumer(
            ILogger<ErrorEventConsumer> logger,
            RecoveryService recoveryService,
            ErrorClassificationService errorClassificationService,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recoveryService = recoveryService ?? throw new ArgumentNullException(nameof(recoveryService));
            _errorClassificationService = errorClassificationService ?? throw new ArgumentNullException(nameof(errorClassificationService));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Consumes an error event.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Consume(ConsumeContext<ErrorEvent> context)
        {
            var errorEvent = context.Message;
            var errorContext = errorEvent.ErrorContext;

            _logger.LogInformation("Consuming error event {CommandId} for error {ErrorCode} from service {ServiceId}",
                errorEvent.CommandId, errorContext.ErrorCode, errorContext.ServiceId);

            try
            {
                // Track the error
                _errorClassificationService.TrackError(errorContext);

                // Handle the error
                var recoveryResult = _recoveryService.HandleError(errorContext, errorEvent.ExecutionContext);

                // Create the result
                var result = new ErrorEventResult
                {
                    ResultId = Guid.NewGuid().ToString(),
                    Success = recoveryResult.Success,
                    RecoveryResult = recoveryResult,
                    RecoveryAction = recoveryResult.Action,
                    NextRetryDelay = recoveryResult.NextRetryDelay
                };

                // Publish the result
                await _messageBus.PublishAsync(result);

                _logger.LogInformation("Published error event result {ResultId} with recovery action {RecoveryAction}",
                    result.ResultId, result.RecoveryAction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling error event {CommandId}", errorEvent.CommandId);

                // Publish a failed result
                var result = new ErrorEventResult
                {
                    ResultId = Guid.NewGuid().ToString(),
                    Success = false,
                    Error = new Abstractions.Common.ExecutionError
                    {
                        ErrorCode = "RECOVERY_ERROR",
                        ErrorMessage = $"Error handling error event: {ex.Message}",
                        ErrorDetails = new Dictionary<string, object>
                        {
                            ["ExceptionType"] = ex.GetType().Name,
                            ["StackTrace"] = ex.StackTrace ?? string.Empty
                        }
                    },
                    RecoveryAction = RecoveryAction.FAIL_EXECUTION
                };

                await _messageBus.PublishAsync(result);
            }
        }
    }
}
