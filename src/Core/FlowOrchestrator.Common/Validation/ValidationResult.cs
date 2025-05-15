namespace FlowOrchestrator.Common.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Gets the validation warnings.
    /// </summary>
    public List<string> Warnings { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">A value indicating whether the validation was successful.</param>
    /// <param name="message">The validation message.</param>
    /// <param name="errors">The validation errors.</param>
    /// <param name="warnings">The validation warnings.</param>
    public ValidationResult(bool isValid, string message, List<string>? errors = null, List<string>? warnings = null)
    {
        IsValid = isValid;
        Message = message;
        Errors = errors ?? new List<string>();
        Warnings = warnings ?? new List<string>();
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="message">The validation message.</param>
    /// <returns>A successful validation result.</returns>
    public static ValidationResult Success(string message = "Validation successful.")
    {
        return new ValidationResult(true, message);
    }

    /// <summary>
    /// Creates a validation result with warnings.
    /// </summary>
    /// <param name="message">The validation message.</param>
    /// <param name="warnings">The validation warnings.</param>
    /// <returns>A validation result with warnings.</returns>
    public static ValidationResult Warning(string message, params string[] warnings)
    {
        return new ValidationResult(true, message, null, warnings.ToList());
    }

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    /// <param name="message">The validation message.</param>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Error(string message, params string[] errors)
    {
        var errorList = new List<string>();
        if (errors.Length > 0)
        {
            errorList.AddRange(errors);
        }
        else
        {
            errorList.Add(message);
        }

        return new ValidationResult(false, message, errorList);
    }

    /// <summary>
    /// Combines multiple validation results into a single result.
    /// </summary>
    /// <param name="results">The validation results to combine.</param>
    /// <returns>A combined validation result.</returns>
    public static ValidationResult Combine(IEnumerable<ValidationResult> results)
    {
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            return new ValidationResult(true, "Combined validation successful.");
        }

        var isValid = resultsList.All(r => r.IsValid);
        var errors = resultsList.SelectMany(r => r.Errors).ToList();
        var warnings = resultsList.SelectMany(r => r.Warnings).ToList();
        var message = isValid ? "Combined validation successful." : "Combined validation failed.";

        return new ValidationResult(isValid, message, errors, warnings);
    }
}
