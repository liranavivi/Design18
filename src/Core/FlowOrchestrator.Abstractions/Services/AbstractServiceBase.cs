using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Statistics;
using Microsoft.Extensions.Logging;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Abstract base class for all services in the FlowOrchestrator system.
    /// Provides common functionality and telemetry integration.
    /// </summary>
    public abstract class AbstractServiceBase : IService
    {
        /// <summary>
        /// The current state of the service.
        /// </summary>
        protected ServiceState _state = ServiceState.UNINITIALIZED;

        /// <summary>
        /// The statistics provider for telemetry.
        /// </summary>
        protected readonly IStatisticsProvider _statisticsProvider;

        /// <summary>
        /// The logger for the service.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractServiceBase"/> class.
        /// </summary>
        /// <param name="statisticsProvider">The statistics provider.</param>
        /// <param name="logger">The logger.</param>
        protected AbstractServiceBase(
            IStatisticsProvider statisticsProvider,
            ILogger logger)
        {
            _statisticsProvider = statisticsProvider;
            _logger = logger;
        }

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
        public abstract string ServiceType { get; }

        /// <summary>
        /// Initializes the service with the specified parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public virtual void Initialize(ConfigurationParameters parameters)
        {
            try
            {
                _statisticsProvider.StartOperation("service.initialize");
                _logger.LogInformation("Initializing {ServiceType} with ID {ServiceId}", ServiceType, ServiceId);

                // Validate configuration
                var validationResult = ValidateConfiguration(parameters);
                if (!validationResult.IsValid)
                {
                    _statisticsProvider.RecordMetric("service.configuration.validation.errors", validationResult.Errors.Count);
                    throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
                }

                // Call the hook for custom initialization logic
                OnInitialize(parameters);

                // Call the hook for when the service is ready
                OnReady();

                _state = ServiceState.READY;
                _statisticsProvider.RecordMetric("service.state", (int)_state);
                _statisticsProvider.EndOperation("service.initialize", OperationResult.SUCCESS);
                _logger.LogInformation("{ServiceType} initialized successfully", ServiceType);
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                _statisticsProvider.RecordMetric("service.state", (int)_state);
                OnError(ex);
                _statisticsProvider.EndOperation("service.initialize", OperationResult.FAILURE);
                _logger.LogError(ex, "Failed to initialize {ServiceType}", ServiceType);
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

            try
            {
                _statisticsProvider.StartOperation("service.terminate");
                _logger.LogInformation("Terminating {ServiceType} with ID {ServiceId}", ServiceType, ServiceId);

                _state = ServiceState.TERMINATING;
                _statisticsProvider.RecordMetric("service.state", (int)_state);

                // Call the hook for custom termination logic
                OnTerminate();

                _state = ServiceState.TERMINATED;
                _statisticsProvider.RecordMetric("service.state", (int)_state);
                _statisticsProvider.EndOperation("service.terminate", OperationResult.SUCCESS);
                _logger.LogInformation("{ServiceType} terminated successfully", ServiceType);
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                _statisticsProvider.RecordMetric("service.state", (int)_state);
                OnError(ex);
                _statisticsProvider.EndOperation("service.terminate", OperationResult.FAILURE);
                _logger.LogError(ex, "Failed to terminate {ServiceType}", ServiceType);
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
        /// Sets the state of the service.
        /// </summary>
        /// <param name="newState">The new state.</param>
        protected void SetState(ServiceState newState)
        {
            var oldState = _state;
            _state = newState;
            _statisticsProvider.RecordMetric("service.state", (int)newState);
            _logger.LogInformation("{ServiceType} state changed from {OldState} to {NewState}",
                ServiceType, oldState, newState);
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateConfiguration(ConfigurationParameters parameters);

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        protected abstract void OnInitialize(ConfigurationParameters parameters);

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected abstract void OnReady();

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
