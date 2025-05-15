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
    /// Manager service for scheduled flow entities.
    /// </summary>
    public class ScheduledFlowEntityManager : EntityManagerBase<IScheduledFlowEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledFlowEntityManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public ScheduledFlowEntityManager(ILogger<ScheduledFlowEntityManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "SCHEDULED-FLOW-ENTITY-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "ScheduledFlowEntityManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("ScheduledFlowEntity", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "ScheduledFlowEntity";
        }

        /// <summary>
        /// Validates if the entity type is valid for this manager.
        /// </summary>
        /// <param name="entityType">The entity type to validate.</param>
        /// <returns>true if the entity type is valid; otherwise, false.</returns>
        protected override bool IsValidEntityType(string entityType)
        {
            return entityType.Contains("ScheduledFlow", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected entity type for this manager.
        /// </summary>
        /// <returns>The expected entity type.</returns>
        protected override string GetExpectedEntityType()
        {
            return "ScheduledFlowEntity";
        }

        /// <summary>
        /// Validates entity-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateEntitySpecifics(IScheduledFlowEntity entity)
        {
            var errors = new List<string>();

            // Validate flow entity ID
            if (string.IsNullOrWhiteSpace(entity.FlowEntityId))
            {
                errors.Add("Flow entity ID cannot be null or empty");
            }

            // Validate task scheduler entity ID
            if (string.IsNullOrWhiteSpace(entity.TaskSchedulerEntityId))
            {
                errors.Add("Task scheduler entity ID cannot be null or empty");
            }

            // Validate flow parameters
            if (entity.FlowParameters == null)
            {
                errors.Add("Flow parameters cannot be null");
            }

            // Validate enabled flag
            if (entity.Enabled == null)
            {
                errors.Add("Enabled flag cannot be null");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Scheduled flow entity validation failed", errors.ToArray())
                : ValidationResult.Success("Scheduled flow entity validation successful");
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
