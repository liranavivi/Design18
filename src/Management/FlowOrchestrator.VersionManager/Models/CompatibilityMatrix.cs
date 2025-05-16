using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents a compatibility matrix for a component, defining which other components and versions it is compatible with.
    /// </summary>
    public class CompatibilityMatrix
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
        /// Gets or sets the component version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of compatibility rules.
        /// </summary>
        [JsonPropertyName("compatibleWith")]
        public List<CompatibilityRule> CompatibleWith { get; set; } = new List<CompatibilityRule>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibilityMatrix"/> class.
        /// </summary>
        public CompatibilityMatrix()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompatibilityMatrix"/> class.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="componentId">The component identifier.</param>
        /// <param name="version">The component version.</param>
        public CompatibilityMatrix(ComponentType componentType, string componentId, string version)
        {
            ComponentType = componentType;
            ComponentId = componentId;
            Version = version;
        }

        /// <summary>
        /// Adds a compatibility rule to the matrix.
        /// </summary>
        /// <param name="rule">The compatibility rule to add.</param>
        public void AddCompatibilityRule(CompatibilityRule rule)
        {
            CompatibleWith.Add(rule);
        }

        /// <summary>
        /// Determines whether the specified component type and version are compatible with this component.
        /// </summary>
        /// <param name="componentType">The component type to check.</param>
        /// <param name="version">The version to check.</param>
        /// <returns>True if compatible; otherwise, false.</returns>
        public bool IsCompatibleWith(ComponentType componentType, string version)
        {
            return CompatibleWith.Any(rule => rule.IsCompatible(componentType, version));
        }

        /// <summary>
        /// Gets all compatibility rules for the specified component type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The list of compatibility rules for the specified component type.</returns>
        public IEnumerable<CompatibilityRule> GetCompatibilityRulesForComponentType(ComponentType componentType)
        {
            return CompatibleWith.Where(rule => rule.ComponentType == componentType);
        }
    }
}
