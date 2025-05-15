using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;

namespace FlowOrchestrator.Integration.Protocols
{
    /// <summary>
    /// Abstract base class for all protocol implementations in the FlowOrchestrator system.
    /// Provides common functionality and a standardized implementation pattern.
    /// </summary>
    public abstract class AbstractProtocol : IProtocol
    {
        /// <summary>
        /// Gets the name of the protocol.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the description of the protocol.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the capabilities of the protocol.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public abstract ProtocolCapabilities GetCapabilities();

        /// <summary>
        /// Gets the connection parameters for the protocol.
        /// </summary>
        /// <returns>The connection parameters.</returns>
        public abstract ConnectionParameters GetConnectionParameters();

        /// <summary>
        /// Validates the connection parameters.
        /// </summary>
        /// <param name="parameters">The connection parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public virtual ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters)
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

            var result = new ValidationResult { IsValid = true };
            var connectionParams = GetConnectionParameters();

            // Validate required parameters
            foreach (var requiredParam in connectionParams.RequiredParameters)
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
                else
                {
                    // Validate parameter type
                    var typeValidationResult = ValidateParameterType(requiredParam.Name, value, requiredParam.Type);
                    if (!typeValidationResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(typeValidationResult.Errors);
                    }
                }
            }

            // Validate optional parameters if provided
            foreach (var optionalParam in connectionParams.OptionalParameters)
            {
                if (parameters.TryGetValue(optionalParam.Name, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    // Validate parameter type
                    var typeValidationResult = ValidateParameterType(optionalParam.Name, value, optionalParam.Type);
                    if (!typeValidationResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(typeValidationResult.Errors);
                    }
                }
            }

            // Validate authentication parameters if provided
            foreach (var authParam in connectionParams.AuthenticationParameters)
            {
                if (parameters.TryGetValue(authParam.Name, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    // Validate parameter type
                    var typeValidationResult = ValidateParameterType(authParam.Name, value, authParam.Type);
                    if (!typeValidationResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(typeValidationResult.Errors);
                    }
                }
            }

            // Call protocol-specific validation
            var protocolValidationResult = ValidateProtocolSpecificParameters(parameters);
            if (!protocolValidationResult.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(protocolValidationResult.Errors);
            }

            return result;
        }

        /// <summary>
        /// Creates a protocol handler.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>The protocol handler.</returns>
        public abstract IProtocolHandler CreateHandler(Dictionary<string, string> parameters);

        /// <summary>
        /// Validates protocol-specific parameters.
        /// </summary>
        /// <param name="parameters">The connection parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateProtocolSpecificParameters(Dictionary<string, string> parameters)
        {
            // Base implementation does no additional validation
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates a parameter value against a parameter type.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="paramType">The parameter type.</param>
        /// <returns>The validation result.</returns>
        protected virtual ValidationResult ValidateParameterType(string paramName, string paramValue, ParameterType paramType)
        {
            var result = new ValidationResult { IsValid = true };

            switch (paramType)
            {
                case ParameterType.INTEGER:
                    if (!int.TryParse(paramValue, out _))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "INVALID_PARAMETER_TYPE",
                            Message = $"Parameter '{paramName}' must be an integer."
                        });
                    }
                    break;

                case ParameterType.DECIMAL:
                    if (!decimal.TryParse(paramValue, out _))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "INVALID_PARAMETER_TYPE",
                            Message = $"Parameter '{paramName}' must be a decimal number."
                        });
                    }
                    break;

                case ParameterType.BOOLEAN:
                    if (!bool.TryParse(paramValue, out _))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "INVALID_PARAMETER_TYPE",
                            Message = $"Parameter '{paramName}' must be a boolean (true/false)."
                        });
                    }
                    break;

                case ParameterType.DATE:
                    if (!DateTime.TryParse(paramValue, out _))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            Code = "INVALID_PARAMETER_TYPE",
                            Message = $"Parameter '{paramName}' must be a valid date."
                        });
                    }
                    break;

                case ParameterType.STRING:
                    // All values are valid strings
                    break;

                default:
                    // For unknown types, assume it's valid
                    break;
            }

            return result;
        }
    }
}
