using FlowOrchestrator.Common.Exceptions;

namespace FlowOrchestrator.Common.Utilities;

/// <summary>
/// Provides guard clause methods to validate method arguments.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Guards against a null argument.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
    public static void AgainstNull<T>(T? argument, string paramName) where T : class
    {
        if (argument == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Guards against a null or empty string argument.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is null or empty.</exception>
    public static void AgainstNullOrEmpty(string? argument, string paramName)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }
    }

    /// <summary>
    /// Guards against a null, empty, or whitespace string argument.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is null, empty, or whitespace.</exception>
    public static void AgainstNullOrWhiteSpace(string? argument, string paramName)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
        }
    }

    /// <summary>
    /// Guards against a null or empty collection argument.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is null or empty.</exception>
    public static void AgainstNullOrEmpty<T>(IEnumerable<T>? argument, string paramName)
    {
        if (argument == null || !argument.Any())
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }
    }

    /// <summary>
    /// Guards against an argument that is out of range.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the argument is out of range.</exception>
    public static void AgainstOutOfRange<T>(T argument, string paramName, T min, T max) where T : IComparable<T>
    {
        if (argument.CompareTo(min) < 0 || argument.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}.");
        }
    }

    /// <summary>
    /// Guards against an invalid argument.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is false.</exception>
    public static void AgainstInvalid(bool condition, string paramName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    /// <summary>
    /// Guards against an invalid operation.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="InvalidOperationException">Thrown when the condition is false.</exception>
    public static void AgainstInvalidOperation(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Guards against an invalid configuration.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ConfigurationException">Thrown when the condition is false.</exception>
    public static void AgainstInvalidConfiguration(bool condition, string message)
    {
        if (!condition)
        {
            throw new ConfigurationException(message);
        }
    }
}
