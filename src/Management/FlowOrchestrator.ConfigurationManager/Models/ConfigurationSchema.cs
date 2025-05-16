using FlowOrchestrator.Abstractions.Common;
using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Configuration.Models
{
    /// <summary>
    /// Represents a schema for configuration values.
    /// </summary>
    public class ConfigurationSchema
    {
        /// <summary>
        /// Gets or sets the unique identifier for the schema.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the schema.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the schema.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the parameters defined in this schema.
        /// </summary>
        public List<ConfigurationParameter> Parameters { get; set; } = new List<ConfigurationParameter>();

        /// <summary>
        /// Validates a set of values against this schema.
        /// </summary>
        /// <param name="values">The values to validate.</param>
        /// <returns>A validation result indicating whether the values are valid.</returns>
        public ValidationResult ValidateValues(Dictionary<string, object> values)
        {
            var errors = new List<ValidationError>();

            // Check for required parameters
            foreach (var parameter in Parameters.Where(p => p.Required))
            {
                if (!values.ContainsKey(parameter.Name) || values[parameter.Name] == null)
                {
                    errors.Add(new ValidationError
                    {
                        Code = "REQUIRED_PARAMETER",
                        Message = $"Required parameter '{parameter.Name}' is missing."
                    });
                }
            }

            // Validate parameter types and constraints
            foreach (var kvp in values)
            {
                var parameter = Parameters.FirstOrDefault(p => p.Name == kvp.Key);
                if (parameter == null)
                {
                    // Unknown parameter - if strict validation is enabled, this would be an error
                    continue;
                }

                // Validate type
                if (!IsTypeValid(kvp.Value, parameter.Type))
                {
                    errors.Add(new ValidationError
                    {
                        Code = "INVALID_TYPE",
                        Message = $"Parameter '{kvp.Key}' has invalid type. Expected {parameter.Type}."
                    });
                    continue;
                }

                // Validate constraints
                if (parameter.Type == ParameterType.String && kvp.Value is string stringValue)
                {
                    // Validate string length
                    if (parameter.MinLength.HasValue && stringValue.Length < parameter.MinLength.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "STRING_TOO_SHORT",
                            Message = $"Parameter '{kvp.Key}' is too short. Minimum length is {parameter.MinLength.Value}."
                        });
                    }

                    if (parameter.MaxLength.HasValue && stringValue.Length > parameter.MaxLength.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "STRING_TOO_LONG",
                            Message = $"Parameter '{kvp.Key}' is too long. Maximum length is {parameter.MaxLength.Value}."
                        });
                    }

                    // Validate pattern
                    if (!string.IsNullOrEmpty(parameter.Pattern) && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, parameter.Pattern))
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "PATTERN_MISMATCH",
                            Message = $"Parameter '{kvp.Key}' does not match the required pattern."
                        });
                    }
                }
                else if (parameter.Type == ParameterType.Number && kvp.Value is IConvertible numericValue)
                {
                    double doubleValue;
                    try
                    {
                        doubleValue = Convert.ToDouble(numericValue);
                    }
                    catch
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "INVALID_NUMBER",
                            Message = $"Parameter '{kvp.Key}' is not a valid number."
                        });
                        continue;
                    }

                    // Validate range
                    if (parameter.Minimum.HasValue && doubleValue < parameter.Minimum.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "NUMBER_TOO_SMALL",
                            Message = $"Parameter '{kvp.Key}' is too small. Minimum value is {parameter.Minimum.Value}."
                        });
                    }

                    if (parameter.Maximum.HasValue && doubleValue > parameter.Maximum.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "NUMBER_TOO_LARGE",
                            Message = $"Parameter '{kvp.Key}' is too large. Maximum value is {parameter.Maximum.Value}."
                        });
                    }
                }
                else if (parameter.Type == ParameterType.Enum && kvp.Value is string enumValue)
                {
                    // Validate enum value
                    if (parameter.AllowedValues != null && parameter.AllowedValues.Count > 0 && !parameter.AllowedValues.Contains(enumValue))
                    {
                        errors.Add(new ValidationError
                        {
                            Code = "INVALID_ENUM_VALUE",
                            Message = $"Parameter '{kvp.Key}' has invalid value. Allowed values are: {string.Join(", ", parameter.AllowedValues)}."
                        });
                    }
                }
            }

            return errors.Count > 0
                ? new ValidationResult { IsValid = false, Errors = errors }
                : new ValidationResult { IsValid = true, Errors = new List<ValidationError>() };
        }

        private bool IsTypeValid(object value, ParameterType expectedType)
        {
            return expectedType switch
            {
                ParameterType.String => value is string,
                ParameterType.Number => value is int or long or float or double or decimal,
                ParameterType.Boolean => value is bool,
                ParameterType.Enum => value is string,
                ParameterType.Array => value is System.Collections.IEnumerable,
                ParameterType.Object => value is System.Collections.IDictionary,
                _ => false
            };
        }
    }

    /// <summary>
    /// Represents a parameter in a configuration schema.
    /// </summary>
    public class ConfigurationParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        public ParameterType Type { get; set; } = ParameterType.String;

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Gets or sets the default value for the parameter.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for numeric parameters.
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum value for numeric parameters.
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum length for string parameters.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length for string parameters.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the pattern for string parameters.
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Gets or sets the allowed values for enum parameters.
        /// </summary>
        public List<string>? AllowedValues { get; set; }
    }

    /// <summary>
    /// Represents the type of a configuration parameter.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ParameterType
    {
        /// <summary>
        /// String parameter.
        /// </summary>
        String,

        /// <summary>
        /// Numeric parameter.
        /// </summary>
        Number,

        /// <summary>
        /// Boolean parameter.
        /// </summary>
        Boolean,

        /// <summary>
        /// Enumeration parameter.
        /// </summary>
        Enum,

        /// <summary>
        /// Array parameter.
        /// </summary>
        Array,

        /// <summary>
        /// Object parameter.
        /// </summary>
        Object
    }
}
