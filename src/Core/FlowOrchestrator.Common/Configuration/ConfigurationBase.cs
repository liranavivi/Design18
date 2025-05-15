namespace FlowOrchestrator.Common.Configuration;

/// <summary>
/// Base class for configuration objects in the FlowOrchestrator system.
/// </summary>
public abstract class ConfigurationBase
{
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
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>A validation result indicating whether the configuration is valid.</returns>
    public abstract Validation.ValidationResult Validate();
}

/// <summary>
/// Interface for configuration objects that can be validated.
/// </summary>
public interface IValidatableConfiguration
{
    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>A validation result indicating whether the configuration is valid.</returns>
    Validation.ValidationResult Validate();
}

/// <summary>
/// Interface for configuration objects that can be serialized.
/// </summary>
public interface ISerializableConfiguration
{
    /// <summary>
    /// Serializes the configuration to a string.
    /// </summary>
    /// <returns>A string representation of the configuration.</returns>
    string Serialize();

    /// <summary>
    /// Deserializes a string to a configuration object.
    /// </summary>
    /// <param name="serializedConfiguration">The serialized configuration.</param>
    /// <returns>True if deserialization was successful, otherwise false.</returns>
    bool Deserialize(string serializedConfiguration);
}
