using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Common.Configuration;
using FlowOrchestrator.Common.Validation;
using System.Text.Json.Serialization;
using ValidationResult = FlowOrchestrator.Common.Validation.ValidationResult;

namespace FlowOrchestrator.Management.Configuration.Models
{
    /// <summary>
    /// Represents a configuration entry in the system.
    /// </summary>
    public class ConfigurationEntry : ConfigurationBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for the configuration entry.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the scope of the configuration (System, Service, Flow, etc.).
        /// </summary>
        public ConfigurationScope Scope { get; set; } = ConfigurationScope.System;

        /// <summary>
        /// Gets or sets the target identifier (service ID, flow ID, etc.) if applicable.
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the environment this configuration applies to.
        /// </summary>
        public string Environment { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the configuration values.
        /// </summary>
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the schema for this configuration.
        /// </summary>
        public ConfigurationSchema? Schema { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user who created this configuration.
        /// </summary>
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// Gets or sets the user who last updated this configuration.
        /// </summary>
        public string UpdatedBy { get; set; } = "System";

        /// <summary>
        /// Gets or sets a value indicating whether this configuration is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <returns>A validation result indicating whether the configuration is valid.</returns>
        public override ValidationResult Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(Version))
            {
                errors.Add("Version is required.");
            }

            if (Scope == ConfigurationScope.Service || Scope == ConfigurationScope.Flow)
            {
                if (string.IsNullOrWhiteSpace(TargetId))
                {
                    errors.Add($"TargetId is required for {Scope} scope.");
                }
            }

            if (Schema != null)
            {
                var schemaValidation = Schema.ValidateValues(Values);
                if (!schemaValidation.IsValid)
                {
                    errors.AddRange(schemaValidation.Errors.Select(e => e.Message));
                }
            }

            return errors.Count > 0
                ? ValidationResult.Error("Configuration validation failed", errors.ToArray())
                : ValidationResult.Success("Configuration validation successful");
        }
    }

    /// <summary>
    /// Represents the scope of a configuration entry.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConfigurationScope
    {
        /// <summary>
        /// System-wide configuration.
        /// </summary>
        System,

        /// <summary>
        /// Service-specific configuration.
        /// </summary>
        Service,

        /// <summary>
        /// Flow-specific configuration.
        /// </summary>
        Flow,

        /// <summary>
        /// Environment-specific configuration.
        /// </summary>
        Environment
    }
}
