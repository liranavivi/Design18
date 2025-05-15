using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Entities
{
    /// <summary>
    /// Defines the interface for versionable objects in the FlowOrchestrator system.
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// Gets or sets the version of the object.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the object was created.
        /// </summary>
        DateTime CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the object was last modified.
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
        /// Creates a new version of the object.
        /// </summary>
        /// <param name="versionDescription">The description of the new version.</param>
        /// <returns>The new version of the object.</returns>
        IVersionable CreateNewVersion(string versionDescription);

        /// <summary>
        /// Gets the version history of the object.
        /// </summary>
        /// <returns>The version history.</returns>
        List<VersionInfo> GetVersionHistory();
    }

    /// <summary>
    /// Represents information about a version.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the version was created.
        /// </summary>
        public DateTime CreatedTimestamp { get; set; }

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
        public VersionStatus VersionStatus { get; set; }
    }
}
