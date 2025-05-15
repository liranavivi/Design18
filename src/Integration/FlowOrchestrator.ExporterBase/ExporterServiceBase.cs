using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using MassTransit;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;

namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Base implementation of the AbstractExporterService that provides common functionality for all exporter services.
    /// </summary>
    public abstract class ExporterServiceBase : AbstractExporterService
    {
        /// <summary>
        /// Gets the merge capabilities of the exporter service.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        public override FlowOrchestrator.Abstractions.Services.MergeCapabilities GetMergeCapabilities()
        {
            return new FlowOrchestrator.Abstractions.Services.MergeCapabilities
            {
                SupportedMergeStrategies = new List<MergeStrategy> { MergeStrategy.CONCATENATE },
                SupportsCustomMergeStrategies = false,
                MaxBranchCount = 10
            };
        }
        /// <summary>
        /// The logger for the service.
        /// </summary>
        protected readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceBase"/> class.
        /// </summary>
        protected ExporterServiceBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceBase"/> class with the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected ExporterServiceBase(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Consumes an export command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Consume(MassTransit.ConsumeContext<ExportCommand> context)
        {
            if (_state != ServiceState.READY)
            {
                _logger?.LogWarning($"Service not in READY state. Current state: {_state}");
                return;
            }

            try
            {
                // Verify that this command is intended for this service
                if (context.Message.ExporterServiceId != ServiceId || context.Message.ExporterServiceVersion != Version)
                {
                    _logger?.LogWarning($"Received export command for different service: {context.Message.ExporterServiceId} (version {context.Message.ExporterServiceVersion})");
                    return;
                }

                // Execute the export operation
                var result = Export(context.Message.Parameters, context.Message.Context);

                // Create and publish the result
                var commandResult = new ExportCommandResult(context.Message, result);
                await PublishResult(commandResult, context.CorrelationId?.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing export command: {ex.Message}");

                // Classify the exception and get error details
                string errorCode = ClassifyException(ex);
                Dictionary<string, object> errorDetails = GetErrorDetails(ex);

                // Create and publish the error
                var error = new ExecutionError
                {
                    ErrorCode = errorCode,
                    ErrorMessage = ex.Message,
                    ErrorDetails = errorDetails
                };

                var commandError = new ExportCommandError(context.Message, error);
                await PublishError(commandError, context.CorrelationId?.ToString());
            }
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override FlowOrchestrator.Abstractions.Common.ValidationResult ValidateParameters(ConfigurationParameters parameters)
        {
            var errors = new List<string>();

            if (parameters == null)
            {
                var result = new FlowOrchestrator.Abstractions.Common.ValidationResult { IsValid = false };
                result.Errors.Add(new ValidationError { Code = "NULL_PARAMETERS", Message = "Parameters cannot be null." });
                return result;
            }

            if (!parameters.Parameters.ContainsKey("ServiceId"))
            {
                errors.Add("ServiceId parameter is required.");
            }

            if (!parameters.Parameters.ContainsKey("Protocol"))
            {
                errors.Add("Protocol parameter is required.");
            }

            if (parameters.TryGetParameter<int>("ConnectionTimeoutSeconds", out var connectionTimeout) && connectionTimeout <= 0)
            {
                errors.Add("ConnectionTimeoutSeconds must be greater than zero.");
            }

            if (parameters.TryGetParameter<int>("OperationTimeoutSeconds", out var operationTimeout) && operationTimeout <= 0)
            {
                errors.Add("OperationTimeoutSeconds must be greater than zero.");
            }

            if (errors.Count > 0)
            {
                var result = new FlowOrchestrator.Abstractions.Common.ValidationResult { IsValid = false };
                foreach (var error in errors)
                {
                    result.Errors.Add(new ValidationError { Code = "VALIDATION_ERROR", Message = error });
                }
                return result;
            }

            return FlowOrchestrator.Abstractions.Common.ValidationResult.Valid();
        }

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected override string ClassifyException(Exception ex)
        {
            return ex switch
            {
                ArgumentException => "INVALID_ARGUMENT",
                InvalidOperationException => "INVALID_OPERATION",
                TimeoutException => "TIMEOUT",
                IOException => "IO_ERROR",
                UnauthorizedAccessException => "UNAUTHORIZED",
                ExporterServiceException exporterEx => exporterEx.ErrorCode,
                _ => "UNKNOWN_ERROR"
            };
        }

        /// <summary>
        /// Gets detailed information about an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        /// <returns>A dictionary of error details.</returns>
        protected override Dictionary<string, object> GetErrorDetails(Exception ex)
        {
            var details = new Dictionary<string, object>
            {
                ["ExceptionType"] = ex.GetType().Name,
                ["Message"] = ex.Message,
                ["StackTrace"] = ex.StackTrace ?? string.Empty
            };

            if (ex is ExporterServiceException exporterEx && exporterEx.ErrorDetails != null)
            {
                foreach (var kvp in exporterEx.ErrorDetails)
                {
                    details[kvp.Key] = kvp.Value;
                }
            }

            return details;
        }

        /// <summary>
        /// Attempts to recover from an error state.
        /// </summary>
        protected override void TryRecover()
        {
            _logger?.LogInformation($"Attempting to recover {ServiceType} service {ServiceId} (version {Version}) from error state");

            try
            {
                // Basic recovery strategy: reinitialize with the current configuration
                if (_configuration != null)
                {
                    Initialize(_configuration);
                    _logger?.LogInformation($"Successfully recovered {ServiceType} service {ServiceId} (version {Version})");
                }
                else
                {
                    _logger?.LogWarning($"Cannot recover {ServiceType} service {ServiceId} (version {Version}) without configuration");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to recover {ServiceType} service {ServiceId} (version {Version}): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger?.LogInformation($"Initializing {ServiceType} service {ServiceId} (version {Version})");
        }

        /// <summary>
        /// Called when the service is ready to process requests.
        /// </summary>
        protected override void OnReady()
        {
            _logger?.LogInformation($"{ServiceType} service {ServiceId} (version {Version}) is ready");
        }

        /// <summary>
        /// Called when the service is processing a request.
        /// </summary>
        protected override void OnProcessing()
        {
            _logger?.LogInformation($"{ServiceType} service {ServiceId} (version {Version}) is processing a request");
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _logger?.LogError(ex, $"{ServiceType} service {ServiceId} (version {Version}) encountered an error: {ex.Message}");
        }

        /// <summary>
        /// Called when the service is being terminated.
        /// </summary>
        protected override void OnTerminate()
        {
            _logger?.LogInformation($"Terminating {ServiceType} service {ServiceId} (version {Version})");
        }

        /// <summary>
        /// Publishes an export command result.
        /// </summary>
        /// <param name="result">The export command result to publish.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task PublishResult(ExportCommandResult result, string? correlationId)
        {
            _logger?.LogInformation($"Publishing export command result for destination {result.Command.DestinationEntityId} (correlation ID: {correlationId})");

            // In a real implementation, this would publish the result to a message bus
            // For now, we'll just log it

            return Task.CompletedTask;
        }

        /// <summary>
        /// Publishes an export command error.
        /// </summary>
        /// <param name="error">The export command error to publish.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task PublishError(ExportCommandError error, string? correlationId)
        {
            _logger?.LogInformation($"Publishing export command error for destination {error.Command.DestinationEntityId} (correlation ID: {correlationId}): {error.Error.ErrorCode} - {error.Error.ErrorMessage}");

            // In a real implementation, this would publish the error to a message bus
            // For now, we'll just log it

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Interface for logging in the exporter service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogInformation(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        /// <param name="message">The message to log.</param>
        void LogError(Exception ex, string message);
    }
}
