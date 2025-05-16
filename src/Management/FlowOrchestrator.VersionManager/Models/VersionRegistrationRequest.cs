using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents a request to register a new version of a component.
    /// </summary>
    public class VersionRegistrationRequest
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
        /// Gets or sets the version information.
        /// </summary>
        [JsonPropertyName("versionInfo")]
        public VersionInfo VersionInfo { get; set; } = new VersionInfo();

        /// <summary>
        /// Gets or sets the compatibility matrix.
        /// </summary>
        [JsonPropertyName("compatibilityMatrix")]
        public CompatibilityMatrix CompatibilityMatrix { get; set; } = new CompatibilityMatrix();
    }
}
