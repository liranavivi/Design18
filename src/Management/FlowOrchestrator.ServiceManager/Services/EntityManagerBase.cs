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
    /// Base class for entity managers.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being managed.</typeparam>
    public abstract class EntityManagerBase<TEntity> : BaseManagerService<TEntity>
        where TEntity : IEntity, IService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManagerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="publishEndpoint">The publish endpoint.</param>
        protected EntityManagerBase(ILogger logger, IPublishEndpoint publishEndpoint)
            : base(logger, publishEndpoint)
        {
        }

        /// <summary>
        /// Validates service-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateServiceSpecifics(TEntity entity)
        {
            var errors = new List<string>();

            // Validate entity ID
            if (string.IsNullOrWhiteSpace(entity.GetEntityId()))
            {
                errors.Add("Entity ID cannot be null or empty");
            }

            // Validate entity type
            if (string.IsNullOrWhiteSpace(entity.GetEntityType()))
            {
                errors.Add("Entity type cannot be null or empty");
            }

            // Validate entity type matches expected type
            if (!string.IsNullOrWhiteSpace(entity.GetEntityType()) && !IsValidEntityType(entity.GetEntityType()))
            {
                errors.Add($"Invalid entity type: {entity.GetEntityType()}. Expected: {GetExpectedEntityType()}");
            }

            // Perform entity-specific validation
            var entityValidation = ValidateEntitySpecifics(entity);
            if (!entityValidation.IsValid)
            {
                errors.AddRange(entityValidation.Errors);
            }

            return errors.Count > 0
                ? ValidationResult.Error("Entity validation failed", errors.ToArray())
                : ValidationResult.Success("Entity validation successful");
        }

        /// <summary>
        /// Validates if the entity type is valid for this manager.
        /// </summary>
        /// <param name="entityType">The entity type to validate.</param>
        /// <returns>true if the entity type is valid; otherwise, false.</returns>
        protected abstract bool IsValidEntityType(string entityType);

        /// <summary>
        /// Gets the expected entity type for this manager.
        /// </summary>
        /// <returns>The expected entity type.</returns>
        protected abstract string GetExpectedEntityType();

        /// <summary>
        /// Validates entity-specific aspects.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <returns>The validation result.</returns>
        protected abstract ValidationResult ValidateEntitySpecifics(TEntity entity);
    }
}
