using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Entities;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Base abstract implementation of the IEntity interface.
    /// Provides common functionality for all entities in the system.
    /// </summary>
    public abstract class AbstractEntity : IEntity
    {
        /// <summary>
        /// Gets or sets the version of the entity.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the entity was last modified.
        /// </summary>
        public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the description of the version.
        /// </summary>
        public string VersionDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the previous version.
        /// </summary>
        public string PreviousVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the version.
        /// </summary>
        public VersionStatus VersionStatus { get; set; } = VersionStatus.DRAFT;

        /// <summary>
        /// Gets a value indicating whether the entity has been modified.
        /// </summary>
        protected bool IsModified { get; private set; } = false;

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        /// <returns>The entity identifier.</returns>
        public abstract string GetEntityId();

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <returns>The entity type.</returns>
        public abstract string GetEntityType();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <returns>The validation result.</returns>
        public abstract ValidationResult Validate();

        /// <summary>
        /// Marks the entity as modified and updates the LastModifiedTimestamp.
        /// </summary>
        public void SetModified()
        {
            IsModified = true;
            LastModifiedTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Clears the modified flag.
        /// </summary>
        public void ClearModified()
        {
            IsModified = false;
        }

        /// <summary>
        /// Gets a value indicating whether the entity has been modified.
        /// </summary>
        /// <returns>True if the entity has been modified, otherwise false.</returns>
        public bool GetIsModified()
        {
            return IsModified;
        }

        /// <summary>
        /// Creates a new version of the entity.
        /// </summary>
        /// <param name="versionDescription">The description of the new version.</param>
        /// <returns>A new version of the entity.</returns>
        public virtual AbstractEntity CreateNewVersion(string versionDescription)
        {
            var currentVersion = Version;
            var parts = currentVersion.Split('.');
            
            if (parts.Length != 3 || !int.TryParse(parts[2], out int patch))
            {
                throw new InvalidOperationException($"Invalid version format: {currentVersion}");
            }

            // Increment patch version
            patch++;
            var newVersion = $"{parts[0]}.{parts[1]}.{patch}";

            // Create a new instance with the same type as this
            var newEntity = (AbstractEntity)Activator.CreateInstance(GetType())!;
            
            // Copy properties
            CopyPropertiesTo(newEntity);
            
            // Update version-specific properties
            newEntity.Version = newVersion;
            newEntity.CreatedTimestamp = DateTime.UtcNow;
            newEntity.LastModifiedTimestamp = DateTime.UtcNow;
            newEntity.VersionDescription = versionDescription;
            newEntity.PreviousVersionId = GetEntityId();
            newEntity.VersionStatus = VersionStatus.DRAFT;
            
            return newEntity;
        }

        /// <summary>
        /// Copies the properties of this entity to another entity.
        /// </summary>
        /// <param name="target">The target entity.</param>
        protected virtual void CopyPropertiesTo(AbstractEntity target)
        {
            // Base implementation copies only the common properties
            // Derived classes should override this to copy their specific properties
            target.VersionStatus = VersionStatus;
        }
    }
}
