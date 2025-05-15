namespace FlowOrchestrator.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : FlowOrchestratorException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException() 
        : this("Validation failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValidationException(string message) 
        : this(message, new List<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and validation errors.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="validationErrors">The validation errors.</param>
    public ValidationException(string message, List<string> validationErrors) 
        : base(message, ErrorClassification.VALIDATION_ERROR)
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, Exception innerException) 
        : base(message, ErrorClassification.VALIDATION_ERROR, innerException)
    {
        ValidationErrors = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message, validation errors, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="validationErrors">The validation errors.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, List<string> validationErrors, Exception innerException) 
        : base(message, ErrorClassification.VALIDATION_ERROR, innerException)
    {
        ValidationErrors = validationErrors;
    }
}
