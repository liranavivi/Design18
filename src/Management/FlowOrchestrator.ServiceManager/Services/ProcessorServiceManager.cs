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
    /// Manager service for processor services.
    /// </summary>
    public class ProcessorServiceManager : BaseManagerService<IProcessorService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorServiceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public ProcessorServiceManager(ILogger<ProcessorServiceManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "PROCESSOR-SERVICE-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "ProcessorServiceManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("Processor", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "ProcessorService";
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateServiceSpecifics(IProcessorService service)
        {
            var errors = new List<string>();

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
                    // Validate supported input formats
                    if (capabilities.SupportedInputFormats == null || !capabilities.SupportedInputFormats.Any())
                    {
                        errors.Add("Processor must support at least one input format");
                    }

                    // Validate supported output formats
                    if (capabilities.SupportedOutputFormats == null || !capabilities.SupportedOutputFormats.Any())
                    {
                        errors.Add("Processor must support at least one output format");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting capabilities: {ex.Message}");
            }

            // Validate schemas
            try
            {
                var inputSchema = service.GetInputSchema();
                if (inputSchema == null)
                {
                    errors.Add("GetInputSchema() returned null");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting input schema: {ex.Message}");
            }

            try
            {
                var outputSchema = service.GetOutputSchema();
                if (outputSchema == null)
                {
                    errors.Add("GetOutputSchema() returned null");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error getting output schema: {ex.Message}");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Processor service validation failed", errors.ToArray())
                : ValidationResult.Success("Processor service validation successful");
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
