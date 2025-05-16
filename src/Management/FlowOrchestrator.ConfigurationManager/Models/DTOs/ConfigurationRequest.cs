using System.ComponentModel.DataAnnotations;

namespace FlowOrchestrator.Management.Configuration.Models.DTOs
{
    /// <summary>
    /// Represents a request to create or update a configuration entry.
    /// </summary>
    public class ConfigurationRequest
    {
        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the configuration.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the configuration.
        /// </summary>
        [Required]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the scope of the configuration.
        /// </summary>
        [Required]
        public ConfigurationScope Scope { get; set; } = ConfigurationScope.System;

        /// <summary>
        /// Gets or sets the target identifier (service ID, flow ID, etc.) if applicable.
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the environment this configuration applies to.
        /// </summary>
        [Required]
        public string Environment { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the configuration values.
        /// </summary>
        [Required]
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the schema identifier for this configuration.
        /// </summary>
        public string? SchemaId { get; set; }
    }

    /// <summary>
    /// Represents a request to create a new configuration schema.
    /// </summary>
    public class ConfigurationSchemaRequest
    {
        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the schema.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the schema.
        /// </summary>
        [Required]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the parameters defined in this schema.
        /// </summary>
        [Required]
        public List<ConfigurationParameter> Parameters { get; set; } = new List<ConfigurationParameter>();
    }

    /// <summary>
    /// Represents a request to validate configuration values against a schema.
    /// </summary>
    public class ConfigurationValidationRequest
    {
        /// <summary>
        /// Gets or sets the schema identifier to validate against.
        /// </summary>
        [Required]
        public string SchemaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the values to validate.
        /// </summary>
        [Required]
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    }
}
