using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using System.Text;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;

namespace FlowOrchestrator.Integration.Importers
{
    /// <summary>
    /// Base implementation of the AbstractImporterService that provides common functionality for all importer services.
    /// </summary>
    public abstract class ImporterServiceBase : AbstractImporterService
    {
        /// <summary>
        /// The logger for the service.
        /// </summary>
        protected readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceBase"/> class.
        /// </summary>
        protected ImporterServiceBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceBase"/> class with the specified logger.
        /// </summary>
        /// <param name="logger">The logger for the service.</param>
        protected ImporterServiceBase(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Consumes an import command message.
        /// </summary>
        /// <param name="context">The consume context containing the import command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Consume(ConsumeContext<ImportCommand> context)
        {
            _logger?.LogInformation($"Received import command for source {context.Message.SourceEntityId} (version {context.Message.SourceEntityVersion})");

            try
            {
                // Verify that this command is intended for this service
                if (context.Message.ImporterServiceId != ServiceId || context.Message.ImporterServiceVersion != Version)
                {
                    _logger?.LogWarning($"Received import command for different service: {context.Message.ImporterServiceId} (version {context.Message.ImporterServiceVersion})");
                    return;
                }

                // Execute the import operation
                var result = Import(context.Message.Parameters, context.Message.Context);

                // Create and publish the result
                var commandResult = new ImportCommandResult(context.Message, result);
                await PublishResult(commandResult, context.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing import command: {ex.Message}");

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

                var commandError = new ImportCommandError(context.Message, error);
                await PublishError(commandError, context.CorrelationId);
            }
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
        {
            var errors = new List<string>();

            // Validate required parameters
            if (!parameters.TryGetParameter<string>("ServiceId", out var serviceId) || string.IsNullOrWhiteSpace(serviceId))
            {
                errors.Add("ServiceId is required");
            }

            if (!parameters.TryGetParameter<string>("Protocol", out var protocol) || string.IsNullOrWhiteSpace(protocol))
            {
                errors.Add("Protocol is required");
            }

            // Add additional validation in derived classes

            if (errors.Count > 0)
            {
                return new ValidationResult { IsValid = false, Errors = errors.Select(e => new ValidationError { Message = e }).ToList() };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateParameters(ImportParameters parameters)
        {
            var errors = new List<ValidationError>();

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(parameters.SourceId))
            {
                errors.Add(new ValidationError { Message = "SourceId is required" });
            }

            if (string.IsNullOrWhiteSpace(parameters.SourceLocation))
            {
                errors.Add(new ValidationError { Message = "SourceLocation is required" });
            }

            // Add additional validation in derived classes

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
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
                NotImplementedException => "NOT_IMPLEMENTED",
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
                { "ExceptionType", ex.GetType().Name },
                { "Message", ex.Message },
                { "Source", ex.Source ?? "Unknown" },
                { "StackTrace", ex.StackTrace ?? "Not available" }
            };

            if (ex.InnerException != null)
            {
                details.Add("InnerExceptionType", ex.InnerException.GetType().Name);
                details.Add("InnerExceptionMessage", ex.InnerException.Message);
            }

            return details;
        }

        /// <summary>
        /// Attempts to recover from an error state.
        /// </summary>
        protected override void TryRecover()
        {
            _logger?.LogInformation("Attempting to recover from error state");

            try
            {
                // Basic recovery strategy: reset state to READY
                _state = ServiceState.READY;
                _logger?.LogInformation("Successfully recovered from error state");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to recover from error state");
                _state = ServiceState.ERROR;
            }
        }

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger?.LogInformation($"Initializing {ServiceType} service: {ServiceId} (version {Version})");
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
            _logger?.LogInformation($"Terminating {ServiceType} service: {ServiceId} (version {Version})");
        }

        /// <summary>
        /// Publishes an import command result.
        /// </summary>
        /// <param name="result">The import command result to publish.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task PublishResult(ImportCommandResult result, string? correlationId)
        {
            _logger?.LogInformation($"Publishing import command result for source {result.Command.SourceEntityId} (correlation ID: {correlationId})");

            // In a real implementation, this would publish the result to a message bus
            // For now, we'll just log it

            return Task.CompletedTask;
        }

        /// <summary>
        /// Publishes an import command error.
        /// </summary>
        /// <param name="error">The import command error to publish.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task PublishError(ImportCommandError error, string? correlationId)
        {
            _logger?.LogInformation($"Publishing import command error for source {error.Command.SourceEntityId} (correlation ID: {correlationId}): {error.Error.ErrorCode} - {error.Error.ErrorMessage}");

            // In a real implementation, this would publish the error to a message bus
            // For now, we'll just log it

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Interface for logging in the importer service.
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
