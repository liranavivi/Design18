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
    /// Manager service for task scheduler entities.
    /// </summary>
    public class TaskSchedulerEntityManager : EntityManagerBase<ITaskSchedulerEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSchedulerEntityManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        public TaskSchedulerEntityManager(ILogger<TaskSchedulerEntityManager> logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "TASK-SCHEDULER-ENTITY-MANAGER";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "TaskSchedulerEntityManager";

        /// <summary>
        /// Validates if the service type is valid for this manager.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <returns>true if the service type is valid; otherwise, false.</returns>
        protected override bool IsValidServiceType(string serviceType)
        {
            return serviceType.Contains("TaskSchedulerEntity", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected service type for this manager.
        /// </summary>
        /// <returns>The expected service type.</returns>
        protected override string GetExpectedServiceType()
        {
            return "TaskSchedulerEntity";
        }

        /// <summary>
        /// Validates if the entity type is valid for this manager.
        /// </summary>
        /// <param name="entityType">The entity type to validate.</param>
        /// <returns>true if the entity type is valid; otherwise, false.</returns>
        protected override bool IsValidEntityType(string entityType)
        {
            return entityType.Contains("TaskScheduler", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the expected entity type for this manager.
        /// </summary>
        /// <returns>The expected entity type.</returns>
        protected override string GetExpectedEntityType()
        {
            return "TaskSchedulerEntity";
        }

        /// <summary>
        /// Validates entity-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateEntitySpecifics(ITaskSchedulerEntity entity)
        {
            var errors = new List<string>();

            // Validate scheduler name
            if (string.IsNullOrWhiteSpace(entity.SchedulerName))
            {
                errors.Add("Scheduler name cannot be null or empty");
            }

            // Validate scheduler type
            if (string.IsNullOrWhiteSpace(entity.SchedulerType))
            {
                errors.Add("Scheduler type cannot be null or empty");
            }

            // Validate schedule expression
            if (string.IsNullOrWhiteSpace(entity.ScheduleExpression))
            {
                errors.Add("Schedule expression cannot be null or empty");
            }

            // Validate schedule parameters
            if (entity.ScheduleParameters == null)
            {
                errors.Add("Schedule parameters cannot be null");
            }

            // Validate enabled flag
            if (entity.Enabled == null)
            {
                errors.Add("Enabled flag cannot be null");
            }

            return errors.Count > 0
                ? ValidationResult.Error("Task scheduler entity validation failed", errors.ToArray())
                : ValidationResult.Success("Task scheduler entity validation successful");
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
