using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;

namespace FlowOrchestrator.Integration.Protocols.Protocols
{
    /// <summary>
    /// Implementation of the file protocol.
    /// </summary>
    public class FileProtocol : AbstractProtocol
    {
        /// <summary>
        /// Gets the name of the protocol.
        /// </summary>
        public override string Name => "file";

        /// <summary>
        /// Gets the description of the protocol.
        /// </summary>
        public override string Description => "Protocol for file system operations";

        /// <summary>
        /// Gets the capabilities of the protocol.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public override ProtocolCapabilities GetCapabilities()
        {
            return new ProtocolCapabilities
            {
                SupportsReading = true,
                SupportsWriting = true,
                SupportsStreaming = false,
                SupportsBatching = true,
                SupportedDataFormats = new List<string>
                {
                    "text/plain",
                    "application/json",
                    "application/xml",
                    "application/octet-stream"
                },
                SupportedAuthenticationMethods = new List<string>
                {
                    "none",
                    "windows"
                },
                SupportedEncryptionMethods = new List<string>
                {
                    "none"
                },
                SupportedCompressionMethods = new List<string>
                {
                    "none",
                    "gzip",
                    "zip"
                }
            };
        }

        /// <summary>
        /// Gets the connection parameters for the protocol.
        /// </summary>
        /// <returns>The connection parameters.</returns>
        public override ConnectionParameters GetConnectionParameters()
        {
            return new ConnectionParameters
            {
                RequiredParameters = new List<ParameterDefinition>
                {
                    new ParameterDefinition
                    {
                        Name = "basePath",
                        Description = "The base path for file operations",
                        Type = ParameterType.STRING
                    }
                },
                OptionalParameters = new List<ParameterDefinition>
                {
                    new ParameterDefinition
                    {
                        Name = "filePattern",
                        Description = "The file pattern for read operations (e.g., *.txt)",
                        Type = ParameterType.STRING,
                        DefaultValue = "*.*"
                    },
                    new ParameterDefinition
                    {
                        Name = "recursive",
                        Description = "Whether to search subdirectories recursively",
                        Type = ParameterType.BOOLEAN,
                        DefaultValue = "false"
                    },
                    new ParameterDefinition
                    {
                        Name = "encoding",
                        Description = "The encoding to use for text files",
                        Type = ParameterType.STRING,
                        DefaultValue = "utf-8"
                    },
                    new ParameterDefinition
                    {
                        Name = "bufferSize",
                        Description = "The buffer size for file operations",
                        Type = ParameterType.INTEGER,
                        DefaultValue = "4096"
                    }
                }
            };
        }

        /// <summary>
        /// Creates a protocol handler.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>The protocol handler.</returns>
        public override IProtocolHandler CreateHandler(Dictionary<string, string> parameters)
        {
            // Validate parameters before creating the handler
            var validationResult = ValidateConnectionParameters(parameters);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Invalid connection parameters: {string.Join(", ", validationResult.Errors.Select(e => e.Message))}");
            }

            return new FileProtocolHandler(parameters);
        }

        /// <summary>
        /// Validates protocol-specific parameters.
        /// </summary>
        /// <param name="parameters">The connection parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateProtocolSpecificParameters(Dictionary<string, string> parameters)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate that the base path exists
            if (parameters.TryGetValue("basePath", out var basePath) && !string.IsNullOrWhiteSpace(basePath))
            {
                if (!Directory.Exists(basePath))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        Code = "INVALID_BASE_PATH",
                        Message = $"Base path '{basePath}' does not exist."
                    });
                }
            }

            return result;
        }
    }
}
