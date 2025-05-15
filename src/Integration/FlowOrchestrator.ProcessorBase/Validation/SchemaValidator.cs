using FlowOrchestrator.Abstractions.Common;
using System.Text.Json;
using ValidationResult = FlowOrchestrator.Abstractions.Common.ValidationResult;
using ValidationError = FlowOrchestrator.Abstractions.Common.ValidationError;
using SchemaField = FlowOrchestrator.Abstractions.Common.SchemaField;

namespace FlowOrchestrator.Processing.Validation
{
    /// <summary>
    /// Provides schema validation functionality for processor services.
    /// </summary>
    public class SchemaValidator
    {
        /// <summary>
        /// Validates data against a schema definition.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateDataAgainstSchema(DataPackage data, SchemaDefinition schema)
        {
            if (data == null)
            {
                return ValidationResult.Error("Data package cannot be null.");
            }

            if (schema == null)
            {
                return ValidationResult.Error("Schema definition cannot be null.");
            }

            var result = new ValidationResult { IsValid = true };

            try
            {
                // Convert data content to JSON for validation
                // This is a simplified approach - in a real implementation,
                // we would handle different content types differently
                var jsonContent = ConvertToJsonObject(data.Content);

                // Validate against schema fields
                foreach (var field in schema.Fields)
                {
                    if (jsonContent.TryGetProperty(field.Name, out var fieldValue))
                    {
                        var fieldValidationResult = ValidateField(field, fieldValue);
                        if (!fieldValidationResult.IsValid)
                        {
                            result.IsValid = false;
                            result.Errors.AddRange(fieldValidationResult.Errors);
                        }
                    }
                    else if (field.Required)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "MISSING_REQUIRED_FIELD",
                            Message = $"Required field '{field.Name}' is missing."
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"Schema validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates a field value against a schema field definition.
        /// </summary>
        /// <param name="field">The schema field definition.</param>
        /// <param name="value">The field value to validate.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateField(SchemaField field, JsonElement value)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate type
            if (!ValidateType(field.Type, value))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "TYPE_MISMATCH",
                    Message = $"Field '{field.Name}' has invalid type. Expected '{field.Type}'."
                });
                return result;
            }

            // Validate additional rules if present
            if (field.ValidationRules != null && field.ValidationRules.Count > 0)
            {
                foreach (var rule in field.ValidationRules)
                {
                    var ruleValidationResult = ValidateRule(field, value, rule.Key, rule.Value);
                    if (!ruleValidationResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(ruleValidationResult.Errors);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validates a field value against a validation rule.
        /// </summary>
        /// <param name="field">The schema field definition.</param>
        /// <param name="value">The field value to validate.</param>
        /// <param name="ruleName">The name of the validation rule.</param>
        /// <param name="ruleValue">The value of the validation rule.</param>
        /// <returns>The validation result.</returns>
        private static ValidationResult ValidateRule(SchemaField field, JsonElement value, string ruleName, object ruleValue)
        {
            var result = new ValidationResult { IsValid = true };

            switch (ruleName.ToLowerInvariant())
            {
                case "minlength":
                    if (value.ValueKind == JsonValueKind.String && value.GetString().Length < Convert.ToInt32(ruleValue))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "MIN_LENGTH_VIOLATION",
                            Message = $"Field '{field.Name}' must have a minimum length of {ruleValue}."
                        });
                    }
                    break;

                case "maxlength":
                    if (value.ValueKind == JsonValueKind.String && value.GetString().Length > Convert.ToInt32(ruleValue))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "MAX_LENGTH_VIOLATION",
                            Message = $"Field '{field.Name}' must have a maximum length of {ruleValue}."
                        });
                    }
                    break;

                case "min":
                    if (value.ValueKind == JsonValueKind.Number && value.GetDouble() < Convert.ToDouble(ruleValue))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "MIN_VALUE_VIOLATION",
                            Message = $"Field '{field.Name}' must have a minimum value of {ruleValue}."
                        });
                    }
                    break;

                case "max":
                    if (value.ValueKind == JsonValueKind.Number && value.GetDouble() > Convert.ToDouble(ruleValue))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "MAX_VALUE_VIOLATION",
                            Message = $"Field '{field.Name}' must have a maximum value of {ruleValue}."
                        });
                    }
                    break;

                case "pattern":
                    if (value.ValueKind == JsonValueKind.String && !System.Text.RegularExpressions.Regex.IsMatch(value.GetString(), ruleValue.ToString()))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "PATTERN_VIOLATION",
                            Message = $"Field '{field.Name}' must match the pattern '{ruleValue}'."
                        });
                    }
                    break;

                // Add more validation rules as needed
            }

            return result;
        }

        /// <summary>
        /// Validates that a value matches the expected type.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if the value matches the expected type; otherwise, false.</returns>
        private static bool ValidateType(string expectedType, JsonElement value)
        {
            switch (expectedType.ToLowerInvariant())
            {
                case "string":
                    return value.ValueKind == JsonValueKind.String;
                case "integer":
                    return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _);
                case "number":
                case "float":
                case "double":
                    return value.ValueKind == JsonValueKind.Number;
                case "boolean":
                    return value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False;
                case "object":
                    return value.ValueKind == JsonValueKind.Object;
                case "array":
                    return value.ValueKind == JsonValueKind.Array;
                case "null":
                    return value.ValueKind == JsonValueKind.Null;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts an object to a JSON element for validation.
        /// </summary>
        /// <param name="content">The content to convert.</param>
        /// <returns>The JSON element.</returns>
        private static JsonElement ConvertToJsonObject(object content)
        {
            if (content is JsonElement jsonElement)
            {
                return jsonElement;
            }

            if (content is string jsonString)
            {
                try
                {
                    return JsonDocument.Parse(jsonString).RootElement;
                }
                catch
                {
                    // If it's not valid JSON, serialize it as a string value
                    var jsonObj = new { value = jsonString };
                    var json = JsonSerializer.Serialize(jsonObj);
                    return JsonDocument.Parse(json).RootElement.GetProperty("value");
                }
            }

            // For other types, serialize to JSON first
            var jsonContent = JsonSerializer.Serialize(content);
            return JsonDocument.Parse(jsonContent).RootElement;
        }
    }
}
