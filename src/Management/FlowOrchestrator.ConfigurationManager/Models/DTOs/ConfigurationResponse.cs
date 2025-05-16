using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Management.Configuration.Models.DTOs
{
    /// <summary>
    /// Represents a response containing a configuration entry.
    /// </summary>
    public class ConfigurationResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the configuration entry.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the configuration.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the configuration.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scope of the configuration.
        /// </summary>
        public ConfigurationScope Scope { get; set; }

        /// <summary>
        /// Gets or sets the target identifier (service ID, flow ID, etc.) if applicable.
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the environment this configuration applies to.
        /// </summary>
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the configuration values.
        /// </summary>
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the schema identifier for this configuration.
        /// </summary>
        public string? SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created this configuration.
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user who last updated this configuration.
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this configuration is active.
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents a response containing a configuration schema.
    /// </summary>
    public class ConfigurationSchemaResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the schema.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the schema.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the schema.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters defined in this schema.
        /// </summary>
        public List<ConfigurationParameter> Parameters { get; set; } = new List<ConfigurationParameter>();
    }

    /// <summary>
    /// Represents a response containing the result of a configuration validation.
    /// </summary>
    public class ConfigurationValidationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the configuration is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation errors, if any.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    }
}
