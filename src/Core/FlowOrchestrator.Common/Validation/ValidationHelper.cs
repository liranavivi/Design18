using System.Text.RegularExpressions;

namespace FlowOrchestrator.Common.Validation;

/// <summary>
/// Provides helper methods for validation.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidateNotNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ValidationResult.Error($"{paramName} cannot be null or empty.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidateNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Error($"{paramName} cannot be null, empty, or whitespace.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a value is not null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the value is valid.</returns>
    public static ValidationResult ValidateNotNull(object? value, string paramName)
    {
        if (value == null)
        {
            return ValidationResult.Error($"{paramName} cannot be null.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a value is within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the value is valid.</returns>
    public static ValidationResult ValidateRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            return ValidationResult.Error($"{paramName} must be between {min} and {max}.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidatePattern(string value, string pattern, string paramName)
    {
        if (!Regex.IsMatch(value, pattern))
        {
            return ValidationResult.Error($"{paramName} does not match the required pattern.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the collection is valid.</returns>
    public static ValidationResult ValidateNotNullOrEmpty<T>(IEnumerable<T>? collection, string paramName)
    {
        if (collection == null || !collection.Any())
        {
            return ValidationResult.Error($"{paramName} cannot be null or empty.");
        }

        return ValidationResult.Success();
    }
}
