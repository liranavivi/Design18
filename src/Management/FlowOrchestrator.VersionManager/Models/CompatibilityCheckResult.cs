using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents the result of a compatibility check between two component versions.
    /// </summary>
    public class CompatibilityCheckResult
    {
        /// <summary>
        /// Gets or sets the source component type.
        /// </summary>
        [JsonPropertyName("sourceType")]
        public ComponentType SourceType { get; set; }

        /// <summary>
        /// Gets or sets the source component identifier.
        /// </summary>
        [JsonPropertyName("sourceId")]
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source component version.
        /// </summary>
        [JsonPropertyName("sourceVersion")]
        public string SourceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target component type.
        /// </summary>
        [JsonPropertyName("targetType")]
        public ComponentType TargetType { get; set; }

        /// <summary>
        /// Gets or sets the target component identifier.
        /// </summary>
        [JsonPropertyName("targetId")]
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target component version.
        /// </summary>
        [JsonPropertyName("targetVersion")]
        public string TargetVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the components are compatible.
        /// </summary>
        [JsonPropertyName("isCompatible")]
        public bool IsCompatible { get; set; }
    }
}
