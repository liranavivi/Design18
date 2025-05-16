using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Flows.Services
{
    /// <summary>
    /// Base class for flow manager services.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being managed.</typeparam>
    public abstract class BaseFlowManagerService<TEntity> : AbstractManagerService<TEntity, string>
        where TEntity : IEntity, IService
    {
        /// <summary>
        /// The publish endpoint for sending messages.
        /// </summary>
        protected readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlowManagerService{TEntity}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        protected BaseFlowManagerService(ILogger logger, IPublishEndpoint publishEndpoint)
            : base(logger)
        {
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        public override ValidationResult ValidateService(TEntity service)
        {
            if (service == null)
            {
                return ValidationResult.Error("Service cannot be null");
            }

            var errors = new List<string>();

            // Validate service ID
            if (string.IsNullOrWhiteSpace(service.ServiceId))
            {
                errors.Add("Service ID cannot be null or empty");
            }

            // Validate service version
            string serviceVersion = ((IService)service).Version;
            if (string.IsNullOrWhiteSpace(serviceVersion))
            {
                errors.Add("Service version cannot be null or empty");
            }

            // Validate service type
            if (string.IsNullOrWhiteSpace(service.ServiceType))
            {
                errors.Add("Service type cannot be null or empty");
            }

            // Check if the service type is valid for this manager
            if (!string.IsNullOrWhiteSpace(service.ServiceType) && !IsValidServiceType(service.ServiceType))
            {
                errors.Add($"Invalid service type: {service.ServiceType}. Expected: {GetExpectedServiceType()}");
            }

            // Perform additional validation specific to the service type
            var additionalValidation = ValidateServiceSpecifics(service);
            if (!additionalValidation.IsValid)
            {
                errors.AddRange(additionalValidation.Errors);
            }

            return errors.Count > 0
                ? ValidationResult.Error("Service validation failed", errors.ToArray())
                : ValidationResult.Success("Service validation successful");
        }

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
        {
            if (parameters == null)
            {
                return ValidationResult.Error("Configuration parameters cannot be null");
            }

            var errors = new List<string>();

            // Validate storage type
            if (parameters.ContainsKey("StorageType"))
            {
                var storageTypeObj = parameters.Parameters["StorageType"];
                var storageType = storageTypeObj?.ToString();
                if (string.IsNullOrWhiteSpace(storageType))
                {
                    errors.Add("StorageType cannot be null or empty");
                }
                else if (storageType != "InMemory" && storageType != "MongoDB" && storageType != "Hazelcast")
                {
                    errors.Add($"Invalid StorageType: {storageType}. Expected: InMemory, MongoDB, or Hazelcast");
                }
            }

            return errors.Count > 0
                ? ValidationResult.Error("Configuration validation failed", errors.ToArray())
                : ValidationResult.Success("Configuration validation successful");
        }

        /// <summary>
        /// Called when the service is initializing.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger.LogInformation("Initializing {ServiceType} manager", typeof(TEntity).Name);
        }

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected override void OnReady()
        {
            _logger.LogInformation("{ServiceType} manager is ready", typeof(TEntity).Name);
        }

        /// <summary>
        /// Consumes a service registration command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Consume(MassTransit.ConsumeContext<ServiceRegistrationCommand<TEntity>> context)
        {
            try
            {
                _logger.LogInformation("Received service registration command for service type {ServiceType}", typeof(TEntity).Name);

                var service = context.Message.Service;
                var result = RegisterService(service);

                await context.RespondAsync(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming service registration command for service type {ServiceType}", typeof(TEntity).Name);
                await context.RespondAsync(new FlowOrchestrator.Abstractions.Services.ServiceRegistrationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _logger.LogError(ex, "Error in {ServiceType} manager", typeof(TEntity).Name);
        }

        /// <summary>
        /// Called when the service is terminating.
        /// </summary>
        protected override void OnTerminate()
        {
            _logger.LogInformation("Terminating {ServiceType} manager", typeof(TEntity).Name);
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        protected abstract ValidationResult ValidateServiceSpecifics(TEntity service);

        /// <summary>
        /// Gets the expected service type.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected abstract string GetExpectedServiceType();

        /// <summary>
        /// Determines whether the specified service type is valid.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service type is valid; otherwise, false.</returns>
        protected abstract bool IsValidServiceType(string serviceType);
    }
}
