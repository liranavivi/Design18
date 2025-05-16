namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public List<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the validation warnings.
        /// </summary>
        public List<string> Warnings { get; private set; } = new List<string>();

        /// <summary>
        /// Creates a valid validation result.
        /// </summary>
        /// <returns>A valid validation result.</returns>
        public static ValidationResult Valid()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates an invalid validation result with the specified errors.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        /// <returns>An invalid validation result.</returns>
        public static ValidationResult Invalid(IEnumerable<string> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        /// <summary>
        /// Creates an invalid validation result with the specified error.
        /// </summary>
        /// <param name="error">The validation error.</param>
        /// <returns>An invalid validation result.</returns>
        public static ValidationResult Invalid(string error)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { error }
            };
        }

        /// <summary>
        /// Creates a valid validation result with the specified warnings.
        /// </summary>
        /// <param name="warnings">The validation warnings.</param>
        /// <returns>A valid validation result with warnings.</returns>
        public static ValidationResult ValidWithWarnings(IEnumerable<string> warnings)
        {
            return new ValidationResult
            {
                IsValid = true,
                Warnings = warnings.ToList()
            };
        }

        /// <summary>
        /// Creates a valid validation result with the specified warning.
        /// </summary>
        /// <param name="warning">The validation warning.</param>
        /// <returns>A valid validation result with a warning.</returns>
        public static ValidationResult ValidWithWarning(string warning)
        {
            return new ValidationResult
            {
                IsValid = true,
                Warnings = new List<string> { warning }
            };
        }

        /// <summary>
        /// Combines this validation result with another validation result.
        /// </summary>
        /// <param name="other">The other validation result.</param>
        /// <returns>A combined validation result.</returns>
        public ValidationResult Combine(ValidationResult other)
        {
            if (other == null)
            {
                return this;
            }

            var result = new ValidationResult
            {
                IsValid = IsValid && other.IsValid,
                Errors = new List<string>(Errors),
                Warnings = new List<string>(Warnings)
            };

            result.Errors.AddRange(other.Errors);
            result.Warnings.AddRange(other.Warnings);

            return result;
        }
    }
}
