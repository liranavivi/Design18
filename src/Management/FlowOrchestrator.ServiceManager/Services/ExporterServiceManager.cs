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
    /// Manager service for exporter services.
    /// </summary>
    public class ExporterServiceManager : BaseManagerService<IExporterService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public ExporterServiceManager(ILogger<ExporterServiceManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "EXPORTER-SERVICE-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "ExporterServiceManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("Exporter", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "ExporterService";
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IExporterService service)
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
                    // Validate supported destination types
                    if (capabilities.SupportedDestinationTypes == null || !capabilities.SupportedDestinationTypes.Any())
                    {
                        errors.Add("Exporter must support at least one destination type");
                    }

                    // Validate supported data formats
                    if (capabilities.SupportedDataFormats == null || !capabilities.SupportedDataFormats.Any())
                    {
                        errors.Add("Exporter must support at least one data format");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting capabilities: {ex.Message}");
            }

            // Validate merge capabilities
            try
            {
                var mergeCapabilities = service.GetMergeCapabilities();
                if (mergeCapabilities == null)
                {
                    errors.Add("GetMergeCapabilities() returned null");
                }
                else
                {
                    // Validate supported merge strategies
                    if (mergeCapabilities.SupportedMergeStrategies == null)
                    {
                        errors.Add("SupportedMergeStrategies cannot be null");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting merge capabilities: {ex.Message}");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Exporter service validation failed", errors.ToArray())
                : ValidationResult.Success("Exporter service validation successful");
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
