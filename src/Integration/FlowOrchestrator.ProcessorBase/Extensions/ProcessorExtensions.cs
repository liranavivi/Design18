using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Processing.Validation;

namespace FlowOrchestrator.Processing.Extensions
{
    /// <summary>
    /// Provides extension methods for processor services.
    /// </summary>
    public static class ProcessorExtensions
    {
        /// <summary>
        /// Checks if a processor service is compatible with another processor service.
        /// </summary>
        /// <param name="source">The source processor service.</param>
        /// <param name="target">The target processor service.</param>
        /// <returns>The validation result indicating compatibility.</returns>
        public static ValidationResult IsCompatibleWith(this IProcessorService source, IProcessorService target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // Check if the output schema of the source is compatible with the input schema of the target
            return SchemaCompatibilityChecker.CheckCompatibility(source.GetOutputSchema(), target.GetInputSchema());
        }

        /// <summary>
        /// Creates a processing result with the specified data.
        /// </summary>
        /// <param name="processor">The processor service.</param>
        /// <param name="data">The transformed data.</param>
        /// <param name="success">A value indicating whether the processing was successful.</param>
        /// <param name="metadata">The transformation metadata.</param>
        /// <returns>A new processing result.</returns>
        public static ProcessingResult CreateResult(this IProcessorService processor, DataPackage data, bool success = true, Dictionary<string, object>? metadata = null)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new ProcessingResult
            {
                Success = success,
                TransformedData = data,
                TransformationMetadata = metadata ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Creates an error result with the specified error message.
        /// </summary>
        /// <param name="processor">The processor service.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorDetails">The error details.</param>
        /// <returns>A new processing result with an error.</returns>
        public static ProcessingResult CreateErrorResult(this IProcessorService processor, string errorCode, string errorMessage, Dictionary<string, object>? errorDetails = null)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (string.IsNullOrWhiteSpace(errorCode))
            {
                throw new ArgumentException("Error code cannot be null or empty.", nameof(errorCode));
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));
            }

            return new ProcessingResult
            {
                Success = false,
                ValidationResults = new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = errorCode,
                            Message = errorMessage,
                            Details = errorDetails ?? new Dictionary<string, object>()
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Validates a data package against the input schema of a processor service.
        /// </summary>
        /// <param name="processor">The processor service.</param>
        /// <param name="data">The data to validate.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateInput(this IProcessorService processor, DataPackage data)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (processor is AbstractProcessorService abstractProcessor)
            {
                // Use the internal validation method if available
                var validateInputMethod = abstractProcessor.GetType().GetMethod("ValidateInputSchema", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (validateInputMethod != null)
                {
                    return (ValidationResult)validateInputMethod.Invoke(abstractProcessor, new object[] { data });
                }
            }

            // Fall back to using the schema validator directly
            return SchemaValidator.ValidateDataAgainstSchema(data, processor.GetInputSchema());
        }

        /// <summary>
        /// Validates a data package against the output schema of a processor service.
        /// </summary>
        /// <param name="processor">The processor service.</param>
        /// <param name="data">The data to validate.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateOutput(this IProcessorService processor, DataPackage data)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (processor is AbstractProcessorService abstractProcessor)
            {
                // Use the internal validation method if available
                var validateOutputMethod = abstractProcessor.GetType().GetMethod("ValidateOutputSchema", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (validateOutputMethod != null)
                {
                    return (ValidationResult)validateOutputMethod.Invoke(abstractProcessor, new object[] { data });
                }
            }

            // Fall back to using the schema validator directly
            return SchemaValidator.ValidateDataAgainstSchema(data, processor.GetOutputSchema());
        }
    }
}
