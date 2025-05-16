using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;
// Use ConsumeContext from MassTransit to avoid ambiguity
using ConsumeContext = MassTransit.ConsumeContext;

namespace FlowOrchestrator.Management.Flows.Services
{
    /// <summary>
    /// Abstract base class for all manager services in the FlowOrchestrator system.
    /// Provides common functionality for service registration and discovery.
    /// </summary>
    /// <typeparam name="TService">The type of service being managed.</typeparam>
    /// <typeparam name="TServiceId">The type of service identifier.</typeparam>
    public abstract class AbstractManagerService<TService, TServiceId> : IService, IServiceManager<TService, TServiceId>, MassTransit.IConsumer<ServiceRegistrationCommand<TService>>
        where TService : IService
    {
        /// <summary>
        /// The current state of the service.
        /// </summary>
        protected ServiceState _state = ServiceState.UNINITIALIZED;

        /// <summary>
        /// The logger instance.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The service registry.
        /// </summary>
        protected readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TService>> _registry = new();

        /// <summary>
        /// The service status registry.
        /// </summary>
        protected readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Abstractions.Services.ServiceStatus>> _statusRegistry = new();

        /// <summary>
        /// The configuration parameters.
        /// </summary>
        protected ConfigurationParameters _configuration = new();

        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        string IService.Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public string ServiceType => GetType().Name;

        /// <summary>
        /// Gets the service state.
        /// </summary>
        public ServiceState State => _state;

        /// <summary>
        /// Gets the service state.
        /// </summary>
        /// <returns>The service state.</returns>
        public ServiceState GetState()
        {
            return _state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractManagerService{TService, TServiceId}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        protected AbstractManagerService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes the service with the specified configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public virtual void Initialize(ConfigurationParameters parameters)
        {
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

                _logger.LogInformation("{ServiceType} initialized successfully", ServiceType);
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);
                _logger.LogError(ex, "Failed to initialize {ServiceType}", ServiceType);
                throw;
            }
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        public virtual void Terminate()
        {
            _state = ServiceState.TERMINATING;

            try
            {
                // Call the hook for custom termination logic
                OnTerminate();

                _state = ServiceState.TERMINATED;
                _logger.LogInformation("{ServiceType} terminated successfully", ServiceType);
            }
            catch (Exception ex)
            {
                _state = ServiceState.ERROR;
                OnError(ex);
                _logger.LogError(ex, "Failed to terminate {ServiceType}", ServiceType);
                throw;
            }
        }

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <returns>The result of the registration.</returns>
        public virtual FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult RegisterService(TService service)
        {
            try
            {
                if (service == null)
                {
                    return new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                    {
                        Success = false,
                        ErrorMessage = "Service cannot be null"
                    };
                }

                // Validate the service
                var validationResult = ValidateService(service);
                if (!validationResult.IsValid)
                {
                    return new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                    {
                        Success = false,
                        ErrorMessage = "Service validation failed",
                        ErrorDetails = new Dictionary<string, object>
                        {
                            { "ValidationErrors", validationResult.Errors }
                        }
                    };
                }

                // Get the service ID as string
                string serviceId = service.ServiceId;
                string version = service.Version;

                // Add the service to the registry
                var serviceVersions = _registry.GetOrAdd(serviceId, _ => new ConcurrentDictionary<string, TService>());
                if (!serviceVersions.TryAdd(version, service))
                {
                    return new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                    {
                        Success = false,
                        ServiceId = serviceId,
                        ErrorMessage = "Service with the same ID and version already exists",
                        ErrorDetails = new Dictionary<string, object>
                        {
                            { "ExistingVersion", version }
                        }
                    };
                }

                // Add the service status to the registry
                var statusVersions = _statusRegistry.GetOrAdd(serviceId, _ => new ConcurrentDictionary<string, Abstractions.Services.ServiceStatus>());
                statusVersions.TryAdd(version, Abstractions.Services.ServiceStatus.ACTIVE);

                _logger.LogInformation("Service {ServiceId} version {Version} registered successfully", serviceId, version);

                return new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                {
                    Success = true,
                    ServiceId = serviceId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering service");
                return new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets a service by its identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The service if found; otherwise, null.</returns>
        public virtual TService? GetService(TServiceId serviceId)
        {
            string serviceIdStr = serviceId?.ToString() ?? string.Empty;

            if (_registry.TryGetValue(serviceIdStr, out var serviceVersions))
            {
                // Get the latest version
                var latestVersion = serviceVersions.Keys.OrderByDescending(v => v).FirstOrDefault();
                if (latestVersion != null && serviceVersions.TryGetValue(latestVersion, out var service))
                {
                    return service;
                }
            }

            return default;
        }

        /// <summary>
        /// Gets a service by its identifier and version.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <returns>The service if found; otherwise, null.</returns>
        public virtual TService? GetService(TServiceId serviceId, string version)
        {
            string serviceIdStr = serviceId?.ToString() ?? string.Empty;

            if (_registry.TryGetValue(serviceIdStr, out var serviceVersions) &&
                serviceVersions.TryGetValue(version, out var service))
            {
                return service;
            }

            return default;
        }

        /// <summary>
        /// Gets all services.
        /// </summary>
        /// <returns>The collection of services.</returns>
        public virtual IEnumerable<TService> GetAllServices()
        {
            return _registry.Values
                .SelectMany(v => v.Values)
                .ToList();
        }

        /// <summary>
        /// Gets all versions of a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The collection of service versions.</returns>
        public virtual IEnumerable<TService> GetAllServiceVersions(TServiceId serviceId)
        {
            string serviceIdStr = serviceId?.ToString() ?? string.Empty;

            if (_registry.TryGetValue(serviceIdStr, out var serviceVersions))
            {
                return serviceVersions.Values.ToList();
            }

            return Enumerable.Empty<TService>();
        }

        /// <summary>
        /// Gets services by type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The collection of services of the specified type.</returns>
        public virtual IEnumerable<TService> GetServicesByType(string serviceType)
        {
            return _registry.Values
                .SelectMany(v => v.Values)
                .Where(s => s.ServiceType == serviceType)
                .ToList();
        }

        /// <summary>
        /// Gets services by version.
        /// </summary>
        /// <param name="version">The service version.</param>
        /// <returns>The collection of services with the specified version.</returns>
        public virtual IEnumerable<TService> GetServicesByVersion(string version)
        {
            return _registry.Values
                .SelectMany(v => v.Values)
                .Where(s => s.Version == version)
                .ToList();
        }

        /// <summary>
        /// Determines whether a service with the specified identifier exists.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>True if the service exists; otherwise, false.</returns>
        public virtual bool ServiceExists(TServiceId serviceId)
        {
            string serviceIdStr = serviceId?.ToString() ?? string.Empty;
            return _registry.ContainsKey(serviceIdStr);
        }

        /// <summary>
        /// Gets the number of registered services.
        /// </summary>
        /// <returns>The number of registered services.</returns>
        public virtual int GetServiceCount()
        {
            return _registry.Values.Sum(v => v.Count);
        }

        /// <summary>
        /// Unregisters a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>The result of the unregistration.</returns>
        public virtual ServiceUnregistrationResult UnregisterService(TServiceId serviceId)
        {
            try
            {
                string serviceIdStr = serviceId?.ToString() ?? string.Empty;

                if (!_registry.TryGetValue(serviceIdStr, out var serviceVersions))
                {
                    return new ServiceUnregistrationResult
                    {
                        Success = false,
                        ErrorMessage = $"Service with ID '{serviceIdStr}' not found"
                    };
                }

                // Remove all versions of the service
                if (_registry.TryRemove(serviceIdStr, out _))
                {
                    _statusRegistry.TryRemove(serviceIdStr, out _);
                    _logger.LogInformation("Service {ServiceId} unregistered successfully", serviceIdStr);

                    return new ServiceUnregistrationResult
                    {
                        Success = true
                    };
                }

                return new ServiceUnregistrationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to unregister service with ID '{serviceIdStr}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering service");
                return new ServiceUnregistrationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult ValidateService(TService service);

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateConfiguration(ConfigurationParameters parameters);

        /// <summary>
        /// Called when the service is initializing.
        /// </summary>
        protected abstract void OnInitialize();

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
        /// Called when the service is terminating.
        /// </summary>
        protected abstract void OnTerminate();

        /// <summary>
        /// Consumes a service registration command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Consume(MassTransit.ConsumeContext<ServiceRegistrationCommand<TService>> context);
    }
}
