using System.Collections.Concurrent;
using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Processing
{
    /// <summary>
    /// Abstract base class for all processor services in the FlowOrchestrator system.
    /// Provides common functionality and a standardized implementation pattern.
    /// </summary>
    public abstract class AbstractProcessorService : IProcessorService, IMessageConsumer<ProcessCommand>
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
        /// The metrics collector for the service.
        /// </summary>
        protected readonly ConcurrentDictionary<string, object> _metrics = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public abstract string ServiceId { get; }

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public virtual string ServiceType => "Processor";

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public virtual void Initialize(ConfigurationParameters parameters)
        {
            if (_state != ServiceState.UNINITIALIZED && _state != ServiceState.ERROR)
            {
                throw new InvalidOperationException($"Cannot initialize service in state {_state}");
            }

            _state = ServiceState.INITIALIZING;
            _configuration = parameters ?? new ConfigurationParameters();

            try
            {
                // Validate configuration
                var validationResult = ValidateConfiguration(_configuration);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", validationResult.Errors.Select(e => e.Message))}");
                }

                // Call the derived class initialization hook
                OnInitialize();

                _state = ServiceState.READY;
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);
                throw;
            }
        }

        /// <summary>
        /// Processes data.
        /// </summary>
        /// <param name="parameters">The process parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The processing result.</returns>
        public virtual ProcessingResult Process(ProcessParameters parameters, ExecutionContext context)
        {
            if (_state != ServiceState.READY)
            {
                throw new InvalidOperationException($"Service not in READY state. Current state: {_state}");
            }

            _state = ServiceState.PROCESSING;
            var result = new ProcessingResult();

            try
            {
                // Validate input data against schema
                if (parameters.InputData != null)
                {
                    var validationResult = ValidateInputSchema(parameters.InputData);
                    if (!validationResult.IsValid)
                    {
                        result.Success = false;
                        result.ValidationResults = validationResult;
                        return result;
                    }
                }

                // Call the derived class processing hook
                OnProcessing();

                // Perform the actual processing
                result = PerformProcessing(parameters, context);

                // Validate output data against schema
                if (result.TransformedData != null)
                {
                    var validationResult = ValidateOutputSchema(result.TransformedData);
                    if (!validationResult.IsValid)
                    {
                        result.Success = false;
                        result.ValidationResults = validationResult;
                    }
                }

                _state = ServiceState.READY;
                return result;
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);

                result.Success = false;
                result.ValidationResults = new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = ClassifyException(ex),
                            Message = ex.Message,
                            Details = GetErrorDetails(ex)
                        }
                    }
                };

                // Attempt recovery
                TryRecover();

                return result;
            }
        }

        /// <summary>
        /// Gets the capabilities of the processor service.
        /// </summary>
        /// <returns>The processor capabilities.</returns>
        public abstract ProcessorCapabilities GetCapabilities();

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema definition.</returns>
        public abstract SchemaDefinition GetInputSchema();

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema definition.</returns>
        public abstract SchemaDefinition GetOutputSchema();

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateConfiguration(ConfigurationParameters parameters);

        /// <summary>
        /// Validates the process parameters.
        /// </summary>
        /// <param name="parameters">The process parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public virtual ValidationResult ValidateParameters(ProcessParameters parameters)
        {
            var result = new ValidationResult { IsValid = true };

            if (parameters == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "MISSING_PARAMETERS",
                    Message = "Process parameters cannot be null."
                });
                return result;
            }

            if (parameters.InputData == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "MISSING_INPUT_DATA",
                    Message = "Input data cannot be null."
                });
            }

            return result;
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

            try
            {
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
        /// <returns>The service state.</returns>
        public ServiceState GetState()
        {
            return _state;
        }

        /// <summary>
        /// Consumes a process command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Consume(ConsumeContext<ProcessCommand> context);

        /// <summary>
        /// Performs the actual processing.
        /// </summary>
        /// <param name="parameters">The process parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The processing result.</returns>
        protected abstract ProcessingResult PerformProcessing(ProcessParameters parameters, ExecutionContext context);

        /// <summary>
        /// Validates input data against the input schema.
        /// </summary>
        /// <param name="input">The input data to validate.</param>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateInputSchema(DataPackage input)
        {
            return ValidateDataAgainstSchema(input, GetInputSchema());
        }

        /// <summary>
        /// Validates output data against the output schema.
        /// </summary>
        /// <param name="output">The output data to validate.</param>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateOutputSchema(DataPackage output)
        {
            return ValidateDataAgainstSchema(output, GetOutputSchema());
        }

        /// <summary>
        /// Validates data against a schema definition.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateDataAgainstSchema(DataPackage data, SchemaDefinition schema);

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected abstract string ClassifyException(Exception ex);

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
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
        /// Called when the service is ready.
        /// </summary>
        protected abstract void OnReady();

        /// <summary>
        /// Called when the service is processing data.
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
    }
}
