using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Base implementation of the AbstractManagerService that provides common functionality
    /// for all manager services.
    /// </summary>
    /// <typeparam name="TService">The type of service being managed.</typeparam>
    public abstract class BaseManagerService<TService> : AbstractManagerService<TService, string>
        where TService : IService
    {
        /// <summary>
        /// The publish endpoint for sending messages.
        /// </summary>
        protected readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseManagerService{TService}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        protected BaseManagerService(ILogger logger, IPublishEndpoint publishEndpoint)
            : base(logger)
        {
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult Validate(TService service)
        {
            var errors = new List<string>();

            // Check for null
            if (service == null)
            {
                return ValidationResult.Error("Service cannot be null");
            }

            // Validate service ID
            if (string.IsNullOrWhiteSpace(service.ServiceId))
            {
                errors.Add("Service ID cannot be null or empty");
            }

            // Validate version
            if (string.IsNullOrWhiteSpace(service.Version))
            {
                errors.Add("Service version cannot be null or empty");
            }
            else if (!IsValidVersion(service.Version))
            {
                errors.Add($"Invalid version format: {service.Version}. Expected format: X.Y.Z");
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
            var errors = new List<string>();

            // Check for null
            if (parameters == null)
            {
                return ValidationResult.Error("Configuration parameters cannot be null");
            }

            // Validate required parameters
            if (!parameters.ContainsKey("ServiceName"))
            {
                errors.Add("ServiceName parameter is required");
            }

            // Perform additional validation specific to the manager
            var additionalValidation = ValidateManagerSpecificConfiguration(parameters);
            if (!additionalValidation.IsValid)
            {
                errors.AddRange(additionalValidation.Errors);
            }

            return errors.Count > 0
                ? ValidationResult.Error("Configuration validation failed", errors.ToArray())
                : ValidationResult.Success("Configuration validation successful");
        }

        /// <summary>
        /// Consumes a service registration command.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Consume(MassTransit.ConsumeContext<ServiceRegistrationCommand<TService>> context)
        {
            _logger.LogInformation("Received service registration command for service {ServiceId}", context.Message.Service.ServiceId);

            // Register the service
            var result = RegisterService(context.Message.Service);

            // Publish the result
            await _publishEndpoint.Publish(new ServiceRegistrationResponse
            {
                CorrelationId = context.Message.CorrelationId,
                Success = result.Success,
                ServiceId = result.ServiceId,
                ErrorMessage = result.ErrorMessage,
                ErrorDetails = result.ErrorDetails
            });
        }

        /// <summary>
        /// Called when the service is initializing.
        /// </summary>
        protected override void OnInitialize()
        {
            _logger.LogInformation("Initializing {ServiceType}", ServiceType);
        }

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected override void OnReady()
        {
            _logger.LogInformation("{ServiceType} is ready", ServiceType);
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _logger.LogError(ex, "{ServiceType} encountered an error", ServiceType);
        }

        /// <summary>
        /// Called when the service is terminating.
        /// </summary>
        protected override void OnTerminate()
        {
            _logger.LogInformation("Terminating {ServiceType}", ServiceType);
        }

        /// <summary>
        /// Validates if the version string is in a valid format.
        /// </summary>
        /// <param name="version">The version string to validate.</param>
        /// <returns>true if the version is valid; otherwise, false.</returns>
        protected virtual bool IsValidVersion(string version)
        {
            // Simple version validation (X.Y.Z)
            return System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+\.\d+$");
        }

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected abstract bool IsValidServiceType(string serviceType);

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected abstract string GetExpectedServiceType();

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateServiceSpecifics(TService service);

        /// <summary>
        /// Validates manager-specific configuration.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateManagerSpecificConfiguration(ConfigurationParameters parameters);
    }
}
