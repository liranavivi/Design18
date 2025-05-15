using System.Text;
using System.Text.RegularExpressions;

namespace FlowOrchestrator.Common.Extensions;

/// <summary>
/// Provides extension methods for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether a string is null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether a string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is null, empty, or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Returns a value indicating whether the specified string occurs within this string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The string to seek.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>True if the value parameter occurs within this string; otherwise, false.</returns>
    public static bool Contains(this string source, string value, StringComparison comparison)
    {
        return source.Contains(value, comparison);
    }

    /// <summary>
    /// Truncates a string to a specified length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="suffix">The suffix to append if the string is truncated.</param>
    /// <returns>The truncated string.</returns>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Converts a string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static byte[] ToByteArray(this string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    /// <summary>
    /// Converts a string to a byte array using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static byte[] ToByteArray(this string value, Encoding encoding)
    {
        return encoding.GetBytes(value);
    }

    /// <summary>
    /// Converts a string to title case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to title case.</returns>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var words = value.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                var firstChar = words[i][0];
                if (char.IsLower(firstChar))
                {
                    words[i] = char.ToUpper(firstChar) + words[i].Substring(1);
                }
            }
        }

        return string.Join(" ", words);
    }

    /// <summary>
    /// Converts a string to camel case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to camel case.</returns>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return value.ToLower();
        }

        return char.ToLower(value[0]) + value.Substring(1);
    }

    /// <summary>
    /// Removes all whitespace from a string.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <returns>The string with all whitespace removed.</returns>
    public static string RemoveWhitespace(this string value)
    {
        return Regex.Replace(value, @"\s+", string.Empty);
    }
}
