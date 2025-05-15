using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using System.Collections.Concurrent;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Importers
{
    /// <summary>
    /// Abstract base class for all importer services in the FlowOrchestrator system.
    /// Provides common functionality and a standardized implementation pattern.
    /// </summary>
    public abstract class AbstractImporterService : IImporterService, IMessageConsumer<ImportCommand>
    {
        /// <summary>
        /// The current state of the service.
        /// </summary>
        protected ServiceState _state = ServiceState.UNINITIALIZED;

        /// <summary>
        /// The configuration parameters for the service.
        /// </summary>
        protected ConfigurationParameters _configuration = new ConfigurationParameters();

        /// <summary>
        /// A dictionary of metrics collected during service operation.
        /// </summary>
        protected ConcurrentDictionary<string, object> _metrics = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets the unique identifier for the service.
        /// </summary>
        public abstract string ServiceId { get; }

        /// <summary>
        /// Gets the version of the service.
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        public abstract string ServiceType { get; }

        /// <summary>
        /// Gets the protocol supported by the importer service.
        /// </summary>
        public abstract string Protocol { get; }

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public virtual void Initialize(ConfigurationParameters parameters)
        {
            if (_state != ServiceState.UNINITIALIZED)
            {
                throw new InvalidOperationException($"Service cannot be initialized. Current state: {_state}");
            }

            _state = ServiceState.INITIALIZING;

            try
            {
                // Validate configuration
                var validationResult = ValidateConfiguration(parameters);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
                }

                _configuration = parameters;

                // Call the hook for custom initialization logic
                OnInitialize();

                _state = ServiceState.READY;

                // Call the hook for when the service is ready
                OnReady();
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);
                throw;
            }
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        public virtual void Terminate()
        {
            if (_state == ServiceState.TERMINATED)
            {
                return;
            }

            _state = ServiceState.TERMINATING;

            try
            {
                // Call the hook for custom termination logic
                OnTerminate();

                _state = ServiceState.TERMINATED;
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the current state of the service.
        /// </summary>
        /// <returns>The current service state.</returns>
        public ServiceState GetState()
        {
            return _state;
        }

        /// <summary>
        /// Gets the capabilities of the importer service.
        /// </summary>
        /// <returns>The importer capabilities.</returns>
        public abstract ImporterCapabilities GetCapabilities();

        /// <summary>
        /// Imports data from a source.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        public virtual ImportResult Import(ImportParameters parameters, ExecutionContext context)
        {
            if (_state != ServiceState.READY)
            {
                throw new InvalidOperationException($"Service not in READY state. Current state: {_state}");
            }

            _state = ServiceState.PROCESSING;
            StartOperation("import");

            var result = new ImportResult();

            try
            {
                // Call the hook for when the service is processing
                OnProcessing();

                // Perform the import operation
                result = PerformImport(parameters, context);

                _state = ServiceState.READY;
                EndOperation("import", true);

                return result;
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                EndOperation("import", false);

                // Classify the exception and get error details
                string errorCode = ClassifyException(ex);
                Dictionary<string, object> errorDetails = GetErrorDetails(ex);

                // Create an error result
                result.Success = false;
                result.Error = new ExecutionError
                {
                    ErrorCode = errorCode,
                    ErrorMessage = ex.Message,
                    ErrorDetails = errorDetails
                };

                // Call the error hook
                OnError(ex);

                // Try to recover from the error
                TryRecover();

                return result;
            }
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateConfiguration(ConfigurationParameters parameters);

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateParameters(ImportParameters parameters);

        /// <summary>
        /// Consumes an import command message.
        /// </summary>
        /// <param name="context">The consume context containing the import command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Consume(ConsumeContext<ImportCommand> context);

        /// <summary>
        /// Performs the actual import operation.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        protected abstract ImportResult PerformImport(ImportParameters parameters, ExecutionContext context);

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected abstract string ClassifyException(Exception ex);

        /// <summary>
        /// Gets detailed information about an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        /// <returns>A dictionary of error details.</returns>
        protected abstract Dictionary<string, object> GetErrorDetails(Exception ex);

        /// <summary>
        /// Attempts to recover from an error state.
        /// </summary>
        protected abstract void TryRecover();

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Called when the service is ready to process requests.
        /// </summary>
        protected abstract void OnReady();

        /// <summary>
        /// Called when the service is processing a request.
        /// </summary>
        protected abstract void OnProcessing();

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected abstract void OnError(Exception ex);

        /// <summary>
        /// Called when the service is being terminated.
        /// </summary>
        protected abstract void OnTerminate();

        /// <summary>
        /// Starts tracking an operation for metrics collection.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        protected virtual void StartOperation(string operationName)
        {
            _metrics[$"{operationName}.startTime"] = DateTime.UtcNow;
            _metrics[$"{operationName}.inProgress"] = true;
        }

        /// <summary>
        /// Ends tracking an operation and records metrics.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="success">Whether the operation was successful.</param>
        protected virtual void EndOperation(string operationName, bool success)
        {
            if (_metrics.TryGetValue($"{operationName}.startTime", out var startTimeObj) && startTimeObj is DateTime startTime)
            {
                TimeSpan duration = DateTime.UtcNow - startTime;
                _metrics[$"{operationName}.duration"] = duration.TotalMilliseconds;
            }

            _metrics[$"{operationName}.endTime"] = DateTime.UtcNow;
            _metrics[$"{operationName}.success"] = success;
            _metrics[$"{operationName}.inProgress"] = false;
        }

        /// <summary>
        /// Gets the current metrics for the service.
        /// </summary>
        /// <returns>A dictionary of metrics.</returns>
        protected virtual Dictionary<string, object> GetMetrics()
        {
            return new Dictionary<string, object>(_metrics);
        }
    }
}
