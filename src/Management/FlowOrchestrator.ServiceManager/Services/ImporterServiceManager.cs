using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Common.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Manager service for importer services.
    /// </summary>
    public class ImporterServiceManager : BaseManagerService<IImporterService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public ImporterServiceManager(ILogger<ImporterServiceManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "IMPORTER-SERVICE-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "ImporterServiceManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("Importer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "ImporterService";
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IImporterService service)
        {
            var errors = new List<string>();

            // Validate protocol
            if (string.IsNullOrWhiteSpace(service.Protocol))
            {
                errors.Add("Protocol cannot be null or empty");
            }

            // Validate capabilities
            try
            {
                var capabilities = service.GetCapabilities();
                if (capabilities == null)
                {
                    errors.Add("GetCapabilities() returned null");
                }
                else
                {
                    // Validate supported source types
                    if (capabilities.SupportedSourceTypes == null || !capabilities.SupportedSourceTypes.Any())
                    {
                        errors.Add("Importer must support at least one source type");
                    }

                    // Validate supported data formats
                    if (capabilities.SupportedDataFormats == null || !capabilities.SupportedDataFormats.Any())
                    {
                        errors.Add("Importer must support at least one data format");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting capabilities: {ex.Message}");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Importer service validation failed", errors.ToArray())
                : ValidationResult.Success("Importer service validation successful");
        }

        /// <summary>
        /// Validates manager-specific configuration.
        /// </summary>
        /// <param name="parameters">The configuration parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateManagerSpecificConfiguration(ConfigurationParameters parameters)
        {
            // No additional validation for now
            return ValidationResult.Success("Configuration validation successful");
        }
    }
}
