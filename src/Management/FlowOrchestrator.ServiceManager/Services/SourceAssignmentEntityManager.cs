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
    /// Manager service for source assignment entities.
    /// </summary>
    public class SourceAssignmentEntityManager : EntityManagerBase<ISourceAssignmentEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAssignmentEntityManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public SourceAssignmentEntityManager(ILogger<SourceAssignmentEntityManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "SOURCE-ASSIGNMENT-ENTITY-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "SourceAssignmentEntityManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("SourceAssignmentEntity", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "SourceAssignmentEntity";
        }

        /// <summary>
        /// Validates if the entity type is valid for this manager.
        /// </summary>
        /// <param name="entityType">The entity type to validate.</param>
        /// <returns>true if the entity type is valid; otherwise, false.</returns>
        protected override bool IsValidEntityType(string entityType)
        {
            return entityType.Contains("SourceAssignment", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected entity type for this manager.
        /// </summary>
        /// <returns>The expected entity type.</returns>
        protected override string GetExpectedEntityType()
        {
            return "SourceAssignmentEntity";
        }

        /// <summary>
        /// Validates entity-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateEntitySpecifics(ISourceAssignmentEntity entity)
        {
            var errors = new List<string>();

            // Validate source entity ID
            if (string.IsNullOrWhiteSpace(entity.SourceEntityId))
            {
                errors.Add("Source entity ID cannot be null or empty");
            }

            // Validate importer service ID
            if (string.IsNullOrWhiteSpace(entity.ImporterServiceId))
            {
                errors.Add("Importer service ID cannot be null or empty");
            }

            // Validate importer service version
            if (string.IsNullOrWhiteSpace(entity.ImporterServiceVersion))
            {
                errors.Add("Importer service version cannot be null or empty");
            }

            // Validate assignment parameters
            if (entity.AssignmentParameters == null || !entity.AssignmentParameters.Any())
            {
                errors.Add("Assignment parameters cannot be null or empty");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Source assignment entity validation failed", errors.ToArray())
                : ValidationResult.Success("Source assignment entity validation successful");
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
