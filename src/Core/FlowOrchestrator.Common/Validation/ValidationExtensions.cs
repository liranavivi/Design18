namespace FlowOrchestrator.Common.Validation;

/// <summary>
/// Provides extension methods for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidateNotNullOrEmpty(this string? value, string paramName)
    {
        return ValidationHelper.ValidateNotNullOrEmpty(value, paramName);
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidateNotNullOrWhiteSpace(this string? value, string paramName)
    {
        return ValidationHelper.ValidateNotNullOrWhiteSpace(value, paramName);
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
    public static ValidationResult ValidateRange<T>(this T value, T min, T max, string paramName) where T : IComparable<T>
    {
        return ValidationHelper.ValidateRange(value, min, max, paramName);
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the string is valid.</returns>
    public static ValidationResult ValidatePattern(this string value, string pattern, string paramName)
    {
        return ValidationHelper.ValidatePattern(value, pattern, paramName);
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A validation result indicating whether the collection is valid.</returns>
    public static ValidationResult ValidateNotNullOrEmpty<T>(this IEnumerable<T>? collection, string paramName)
    {
        return ValidationHelper.ValidateNotNullOrEmpty(collection, paramName);
    }

    /// <summary>
    /// Throws an exception if the validation result is not valid.
    /// </summary>
    /// <param name="result">The validation result.</param>
    /// <exception cref="Exceptions.ValidationException">Thrown when the validation result is not valid.</exception>
    public static void ThrowIfInvalid(this ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new Exceptions.ValidationException(result.Message, result.Errors);
        }
    }
}
