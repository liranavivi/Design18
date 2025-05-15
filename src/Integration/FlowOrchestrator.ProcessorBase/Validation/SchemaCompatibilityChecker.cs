using FlowOrchestrator.Abstractions.Common;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using ValidationError = FlowOrchestrator.Abstractions.Common.ValidationError;
using SchemaField = FlowOrchestrator.Abstractions.Common.SchemaField;

namespace FlowOrchestrator.Processing.Validation
{
    /// <summary>
    /// Provides functionality to check compatibility between schemas.
    /// </summary>
    public static class SchemaCompatibilityChecker
    {
        /// <summary>
        /// Checks if a source schema is compatible with a target schema.
        /// </summary>
        /// <param name="source">The source schema.</param>
        /// <param name="target">The target schema.</param>
        /// <returns>The validation result indicating compatibility.</returns>
        public static ValidationResult CheckCompatibility(SchemaDefinition source, SchemaDefinition target)
        {
            if (source == null)
            {
                return ValidationResult.Error("Source schema cannot be null.");
            }

            if (target == null)
            {
                return ValidationResult.Error("Target schema cannot be null.");
            }

            var result = new ValidationResult { IsValid = true };

            // Check if all required fields in the target schema are present in the source schema
            foreach (var targetField in target.Fields.Where(f => f.Required))
            {
                var sourceField = source.Fields.FirstOrDefault(f => f.Name == targetField.Name);
                if (sourceField == null)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MISSING_REQUIRED_FIELD",
                        Message = $"Required field '{targetField.Name}' is missing in the source schema."
                    });
                    continue;
                }

                // Check field type compatibility
                if (!IsTypeCompatible(sourceField.Type, targetField.Type))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        Code = "TYPE_INCOMPATIBILITY",
                        Message = $"Field '{targetField.Name}' has incompatible types. Source: '{sourceField.Type}', Target: '{targetField.Type}'."
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a field in a source schema is compatible with a field in a target schema.
        /// </summary>
        /// <param name="sourceField">The source field.</param>
        /// <param name="targetField">The target field.</param>
        /// <returns>True if the fields are compatible; otherwise, false.</returns>
        public static bool IsFieldCompatible(SchemaField sourceField, SchemaField targetField)
        {
            if (sourceField == null || targetField == null)
            {
                return false;
            }

            // Check type compatibility
            if (!IsTypeCompatible(sourceField.Type, targetField.Type))
            {
                return false;
            }

            // If the target field is required, the source field must also be required
            if (targetField.Required && !sourceField.Required)
            {
                return false;
            }

            // Additional validation rule checks could be added here

            return true;
        }

        /// <summary>
        /// Checks if a source type is compatible with a target type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>True if the types are compatible; otherwise, false.</returns>
        private static bool IsTypeCompatible(string sourceType, string targetType)
        {
            // If types are exactly the same, they are compatible
            if (string.Equals(sourceType, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Define type compatibility rules
            switch (targetType.ToLowerInvariant())
            {
                case "string":
                    // Many types can be converted to string
                    return true;
                case "number":
                case "float":
                case "double":
                    // Integer can be converted to number/float/double
                    return sourceType.ToLowerInvariant() == "integer";
                case "integer":
                    // Only integer is compatible with integer
                    return sourceType.ToLowerInvariant() == "integer";
                case "boolean":
                    // Only boolean is compatible with boolean
                    return sourceType.ToLowerInvariant() == "boolean";
                case "object":
                    // Only object is compatible with object
                    return sourceType.ToLowerInvariant() == "object";
                case "array":
                    // Only array is compatible with array
                    return sourceType.ToLowerInvariant() == "array";
                default:
                    // For unknown types, require exact match
                    return string.Equals(sourceType, targetType, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
