using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Configuration;

/// <summary>
/// Provides validation utilities for configuration objects.
/// </summary>
public static class ConfigurationValidator
{
    /// <summary>
    /// Validates a configuration object.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A validation result indicating whether the configuration is valid.</returns>
    public static ValidationResult Validate(ConfigurationBase configuration)
    {
        if (configuration == null)
        {
            return ValidationResult.Error("Configuration cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            return ValidationResult.Error("Configuration name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(configuration.Version))
        {
            return ValidationResult.Error("Configuration version cannot be empty.");
        }

        return configuration.Validate();
    }

    /// <summary>
    /// Validates a collection of configuration objects.
    /// </summary>
    /// <param name="configurations">The configurations to validate.</param>
    /// <returns>A validation result indicating whether all configurations are valid.</returns>
    public static ValidationResult ValidateAll(IEnumerable<ConfigurationBase> configurations)
    {
        if (configurations == null)
        {
            return ValidationResult.Error("Configurations collection cannot be null.");
        }

        var results = new List<ValidationResult>();
        foreach (var configuration in configurations)
        {
            results.Add(Validate(configuration));
        }

        return ValidationResult.Combine(results);
    }
}
