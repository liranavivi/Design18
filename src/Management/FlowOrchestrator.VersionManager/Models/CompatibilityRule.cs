using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents a compatibility rule between component types and versions.
    /// </summary>
    public class CompatibilityRule
    {
        /// <summary>
        /// Gets or sets the component type that this rule applies to.
        /// </summary>
        [JsonPropertyName("componentType")]
        public ComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the version range that this rule applies to.
        /// </summary>
        [JsonPropertyName("versionRange")]
        public VersionRange VersionRange { get; set; } = new VersionRange();

        /// <summary>
        /// Gets or sets notes about this compatibility rule.
        /// </summary>
        [JsonPropertyName("notes")]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets configuration requirements for this compatibility rule.
        /// </summary>
        [JsonPropertyName("configurationRequirements")]
        public Dictionary<string, object>? ConfigurationRequirements { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibilityRule"/> class.
        /// </summary>
        public CompatibilityRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibilityRule"/> class.
        /// </summary>
        /// <param name="componentType">The component type that this rule applies to.</param>
        /// <param name="versionRange">The version range that this rule applies to.</param>
        /// <param name="notes">Notes about this compatibility rule.</param>
        public CompatibilityRule(ComponentType componentType, VersionRange versionRange, string notes = "")
        {
            ComponentType = componentType;
            VersionRange = versionRange;
            Notes = notes;
        }

        /// <summary>
        /// Determines whether the specified component type and version are compatible with this rule.
        /// </summary>
        /// <param name="componentType">The component type to check.</param>
        /// <param name="version">The version to check.</param>
        /// <returns>True if compatible; otherwise, false.</returns>
        public bool IsCompatible(ComponentType componentType, string version)
        {
            if (componentType != ComponentType)
            {
                return false;
            }

            return VersionRange.IsVersionInRange(version);
        }
    }
}
