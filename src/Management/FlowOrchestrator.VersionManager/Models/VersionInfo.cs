using FlowOrchestrator.Abstractions.Common;
using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents detailed information about a component version.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        [JsonPropertyName("componentType")]
        public ComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        [JsonPropertyName("componentId")]
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the version was created.
        /// </summary>
        [JsonPropertyName("createdTimestamp")]
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the version was last modified.
        /// </summary>
        [JsonPropertyName("lastModifiedTimestamp")]
        public DateTime LastModifiedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the description of the version.
        /// </summary>
        [JsonPropertyName("versionDescription")]
        public string VersionDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the previous version.
        /// </summary>
        [JsonPropertyName("previousVersionId")]
        public string PreviousVersionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the version.
        /// </summary>
        [JsonPropertyName("versionStatus")]
        public VersionStatus VersionStatus { get; set; } = VersionStatus.DRAFT;

        /// <summary>
        /// Gets or sets additional metadata for the version.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// </summary>
        public VersionInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="versionDescription">The version description.</param>
        public VersionInfo(ComponentType componentType, string componentId, string version, string versionDescription)
        {
            ComponentType = componentType;
            ComponentId = componentId;
            Version = version;
            VersionDescription = versionDescription;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VersionInfo"/> class from an Abstractions.Entities.VersionInfo object.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>A new instance of the <see cref="VersionInfo"/> class.</returns>
        public static VersionInfo FromAbstractionsVersionInfo(
            ComponentType componentType, 
            string componentId, 
            FlowOrchestrator.Abstractions.Entities.VersionInfo versionInfo)
        {
            return new VersionInfo
            {
                ComponentType = componentType,
                ComponentId = componentId,
                Version = versionInfo.Version,
                CreatedTimestamp = versionInfo.CreatedTimestamp,
                VersionDescription = versionInfo.VersionDescription,
                PreviousVersionId = versionInfo.PreviousVersionId,
                VersionStatus = versionInfo.VersionStatus
            };
        }
    }
}
