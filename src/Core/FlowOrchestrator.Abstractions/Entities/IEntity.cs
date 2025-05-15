using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Defines the base interface for all entities in the FlowOrchestrator system.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the version of the entity.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the entity was created.
        /// </summary>
        DateTime CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the entity was last modified.
        /// </summary>
        DateTime LastModifiedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the description of the version.
        /// </summary>
        string VersionDescription { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the previous version.
        /// </summary>
        string PreviousVersionId { get; set; }

        /// <summary>
        /// Gets or sets the status of the version.
        /// </summary>
        VersionStatus VersionStatus { get; set; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        string GetEntityId();

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        string GetEntityType();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        ValidationResult Validate();
    }
}
