using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;

namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Abstract base class for all exporter services in the FlowOrchestrator system.
    /// Provides common functionality and a standardized implementation pattern.
    /// </summary>
    public abstract class AbstractExporterService : IExporterService, IConsumer<ExportCommand>
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
        /// The metrics for the service.
        /// </summary>
        protected Dictionary<string, object> _metrics = new Dictionary<string, object>();

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
        /// Gets the protocol used by the service.
        /// </summary>
        public abstract string Protocol { get; }

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public virtual void Initialize(ConfigurationParameters parameters)
        {
            if (_state != ServiceState.UNINITIALIZED && _state != ServiceState.ERROR)
            {
                throw new InvalidOperationException($"Service cannot be initialized in state {_state}");
            }

            _state = ServiceState.INITIALIZING;

            try
            {
                // Validate configuration
                var validationResult = ValidateParameters(parameters);
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
        /// Gets the capabilities of the exporter service.
        /// </summary>
        /// <returns>The exporter capabilities.</returns>
        public abstract ExporterCapabilities GetCapabilities();



        /// <summary>
        /// Exports data to a destination.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        public virtual ExportResult Export(ExportParameters parameters, ExecutionContext context)
        {
            if (_state != ServiceState.READY)
            {
                throw new InvalidOperationException($"Service not in READY state. Current state: {_state}");
            }

            _state = ServiceState.PROCESSING;
            StartOperation("export");

            var result = new ExportResult();

            try
            {
                // Call the hook for when the service is processing
                OnProcessing();

                // Perform the export operation
                result = PerformExport(parameters, context);

                _state = ServiceState.READY;
                EndOperation("export", true);

                return result;
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                EndOperation("export", false);
                OnError(ex);
                throw;
            }
        }

        /// <summary>
        /// Merges data from multiple branches.
        /// </summary>
        /// <param name="branchData">The data from each branch.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        public abstract ExportResult MergeBranches(Dictionary<string, DataPackage> branchData, MergeStrategy strategy, ExecutionContext context);

        /// <summary>
        /// Gets the merge capabilities of the exporter service.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        public abstract FlowOrchestrator.Abstractions.Services.MergeCapabilities GetMergeCapabilities();


        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateParameters(ConfigurationParameters parameters);

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public virtual ValidationResult ValidateParameters(ExportParameters parameters)
        {
            var errors = new List<string>();

            if (parameters == null)
            {
                var result = new ValidationResult { IsValid = false };
                result.Errors.Add(new ValidationError { Code = "NULL_PARAMETERS", Message = "Parameters cannot be null." });
                return result;
            }

            if (parameters.Data == null)
            {
                errors.Add("Data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(parameters.DestinationId))
            {
                errors.Add("DestinationId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(parameters.DestinationLocation))
            {
                errors.Add("DestinationLocation cannot be empty.");
            }

            if (errors.Count > 0)
            {
                var result = new ValidationResult { IsValid = false };
                foreach (var error in errors)
                {
                    result.Errors.Add(new ValidationError { Code = "VALIDATION_ERROR", Message = error });
                }
                return result;
            }

            return ValidationResult.Valid();
        }

        /// <summary>
        /// Consumes an export command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Consume(MassTransit.ConsumeContext<ExportCommand> context);



        /// <summary>
        /// Performs the export operation.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        protected abstract ExportResult PerformExport(ExportParameters parameters, ExecutionContext context);

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
    }
}
