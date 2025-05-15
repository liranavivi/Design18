using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;
using System.Text;
using System.Text.Json;

namespace FlowOrchestrator.Integration.Protocols.Utilities
{
    /// <summary>
    /// Provides utility methods for protocol operations.
    /// </summary>
    public static class ProtocolUtilities
    {
        /// <summary>
        /// Creates a new data package with the specified content and content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="sourceInfo">The source information.</param>
        /// <returns>A new data package.</returns>
        public static DataPackage CreateDataPackage(object content, string contentType, SourceInformation? sourceInfo = null)
        {
            return new DataPackage
            {
                Content = content,
                ContentType = contentType,
                CreatedTimestamp = DateTime.UtcNow,
                Source = sourceInfo
            };
        }

        /// <summary>
        /// Creates a new data package from a byte array.
        /// </summary>
        /// <param name="data">The byte array data.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="sourceInfo">The source information.</param>
        /// <returns>A new data package.</returns>
        public static DataPackage CreateDataPackageFromBytes(byte[] data, string contentType, SourceInformation? sourceInfo = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            object content;

            // Try to parse the data based on the content type
            if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var jsonString = Encoding.UTF8.GetString(data);
                    content = JsonSerializer.Deserialize<JsonElement>(jsonString);
                }
                catch
                {
                    // If parsing fails, use the raw data
                    content = data;
                }
            }
            else if (contentType.Contains("text", StringComparison.OrdinalIgnoreCase) ||
                     contentType.Contains("xml", StringComparison.OrdinalIgnoreCase) ||
                     contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
            {
                content = Encoding.UTF8.GetString(data);
            }
            else
            {
                // For binary data, use the raw bytes
                content = data;
            }

            return CreateDataPackage(content, contentType, sourceInfo);
        }

        /// <summary>
        /// Converts a data package to a byte array.
        /// </summary>
        /// <param name="dataPackage">The data package to convert.</param>
        /// <returns>The byte array representation of the data package.</returns>
        public static byte[] ToBytes(DataPackage dataPackage)
        {
            if (dataPackage == null)
            {
                throw new ArgumentNullException(nameof(dataPackage));
            }

            if (dataPackage.Content == null)
            {
                return Array.Empty<byte>();
            }

            // Handle different content types
            if (dataPackage.Content is byte[] byteArray)
            {
                return byteArray;
            }
            else if (dataPackage.Content is string stringContent)
            {
                return Encoding.UTF8.GetBytes(stringContent);
            }
            else if (dataPackage.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                var jsonString = JsonSerializer.Serialize(dataPackage.Content);
                return Encoding.UTF8.GetBytes(jsonString);
            }
            else
            {
                // For other types, serialize to JSON and convert to bytes
                var jsonString = JsonSerializer.Serialize(dataPackage.Content);
                return Encoding.UTF8.GetBytes(jsonString);
            }
        }

        /// <summary>
        /// Validates connection parameters against a connection parameters definition.
        /// </summary>
        /// <param name="parameters">The connection parameters to validate.</param>
        /// <param name="definition">The connection parameters definition.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters, ConnectionParameters definition)
        {
            if (parameters == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = "NULL_PARAMETERS",
                            Message = "Connection parameters cannot be null."
                        }
                    }
                };
            }

            if (definition == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Code = "NULL_DEFINITION",
                            Message = "Connection parameters definition cannot be null."
                        }
                    }
                };
            }

            var result = new ValidationResult { IsValid = true };

            // Validate required parameters
            foreach (var requiredParam in definition.RequiredParameters)
            {
                if (!parameters.TryGetValue(requiredParam.Name, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MISSING_REQUIRED_PARAMETER",
                        Message = $"Required parameter '{requiredParam.Name}' is missing or empty."
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a source information object from connection parameters.
        /// </summary>
        /// <param name="protocol">The protocol name.</param>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>A new source information object.</returns>
        public static SourceInformation CreateSourceInformation(string protocol, Dictionary<string, string> parameters)
        {
            var sourceInfo = new SourceInformation
            {
                SourceType = protocol
            };

            // Try to extract common source information from parameters
            if (parameters.TryGetValue("sourceId", out var sourceId))
            {
                sourceInfo.SourceId = sourceId;
            }

            if (parameters.TryGetValue("sourceLocation", out var sourceLocation))
            {
                sourceInfo.SourceLocation = sourceLocation;
            }

            return sourceInfo;
        }

        /// <summary>
        /// Gets a parameter value from the connection parameters.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="parameters">The connection parameters.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found or cannot be converted.</param>
        /// <returns>The parameter value if found and convertible; otherwise, the default value.</returns>
        public static T GetParameterValue<T>(Dictionary<string, string> parameters, string paramName, T defaultValue)
        {
            if (parameters == null || string.IsNullOrWhiteSpace(paramName) || !parameters.TryGetValue(paramName, out var stringValue))
            {
                return defaultValue;
            }

            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)stringValue;
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)int.Parse(stringValue);
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)bool.Parse(stringValue);
                }
                else if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)decimal.Parse(stringValue);
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    return (T)(object)DateTime.Parse(stringValue);
                }
                else
                {
                    // For other types, try to convert using the type converter
                    return (T)Convert.ChangeType(stringValue, typeof(T));
                }
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
