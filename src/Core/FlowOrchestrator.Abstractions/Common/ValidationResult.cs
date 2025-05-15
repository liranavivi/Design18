namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the list of validation errors.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// Gets or sets the list of validation warnings.
        /// </summary>
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();

        /// <summary>
        /// Creates a new instance of the <see cref="ValidationResult"/> class with IsValid set to true.
        /// </summary>
        /// <returns>A valid validation result.</returns>
        public static ValidationResult Valid() => new ValidationResult { IsValid = true };

        /// <summary>
        /// Creates a new instance of the <see cref="ValidationResult"/> class with IsValid set to false and the specified error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>An invalid validation result with the specified error.</returns>
        public static ValidationResult Invalid(string errorCode, string errorMessage)
        {
            var result = new ValidationResult { IsValid = false };
            result.Errors.Add(new ValidationError { Code = errorCode, Message = errorMessage });
            return result;
        }

        /// <summary>
        /// Creates a validation result with an error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult Error(string errorMessage)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        Code = "VALIDATION_ERROR",
                        Message = errorMessage
                    }
                }
            };
        }

        /// <summary>
        /// Creates a validation result with an error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult Error(string errorCode, string errorMessage)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        Code = errorCode,
                        Message = errorMessage
                    }
                }
            };
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns>The validation result with the added warning.</returns>
        public ValidationResult WithWarning(string warningCode, string warningMessage)
        {
            Warnings.Add(new ValidationWarning { Code = warningCode, Message = warningMessage });
            return this;
        }
    }

    /// <summary>
    /// Represents a validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property path that caused the error.
        /// </summary>
        public string? PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets additional details about the error.
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a validation warning.
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property path that caused the warning.
        /// </summary>
        public string? PropertyPath { get; set; }
    }
}
