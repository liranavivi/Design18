using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Common.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

// Use ValidationResult from FlowOrchestrator.Common.Validation to avoid ambiguity
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Services
{
    /// <summary>
    /// Manager service for destination assignment entities.
    /// </summary>
    public class DestinationAssignmentEntityManager : EntityManagerBase<IDestinationAssignmentEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationAssignmentEntityManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public DestinationAssignmentEntityManager(ILogger<DestinationAssignmentEntityManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "DESTINATION-ASSIGNMENT-ENTITY-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "DestinationAssignmentEntityManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("DestinationAssignmentEntity", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "DestinationAssignmentEntity";
        }

        /// <summary>
        /// Validates if the entity type is valid for this manager.
        /// </summary>
        /// <param name="entityType">The entity type to validate.</param>
        /// <returns>true if the entity type is valid; otherwise, false.</returns>
        protected override bool IsValidEntityType(string entityType)
        {
            return entityType.Contains("DestinationAssignment", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected entity type for this manager.
        /// </summary>
        /// <returns>The expected entity type.</returns>
        protected override string GetExpectedEntityType()
        {
            return "DestinationAssignmentEntity";
        }

        /// <summary>
        /// Validates entity-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateEntitySpecifics(IDestinationAssignmentEntity entity)
        {
            var errors = new List<string>();

            // Validate destination entity ID
            if (string.IsNullOrWhiteSpace(entity.DestinationEntityId))
            {
                errors.Add("Destination entity ID cannot be null or empty");
            }

            // Validate exporter service ID
            if (string.IsNullOrWhiteSpace(entity.ExporterServiceId))
            {
                errors.Add("Exporter service ID cannot be null or empty");
            }

            // Validate exporter service version
            if (string.IsNullOrWhiteSpace(entity.ExporterServiceVersion))
            {
                errors.Add("Exporter service version cannot be null or empty");
            }

            // Validate assignment parameters
            if (entity.AssignmentParameters == null || !entity.AssignmentParameters.Any())
            {
                errors.Add("Assignment parameters cannot be null or empty");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Destination assignment entity validation failed", errors.ToArray())
                : ValidationResult.Success("Destination assignment entity validation successful");
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
