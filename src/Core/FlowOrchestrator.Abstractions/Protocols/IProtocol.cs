using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Protocols
{
    /// <summary>
    /// Defines the interface for protocols in the FlowOrchestrator system.
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// Gets the name of the protocol.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the protocol.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the capabilities of the protocol.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        ProtocolCapabilities GetCapabilities();

        /// <summary>
        /// Gets the connection parameters for the protocol.
        /// </summary>
        /// <returns>The connection parameters.</returns>
        ConnectionParameters GetConnectionParameters();

        /// <summary>
        /// Validates the connection parameters.
        /// </summary>
        /// <param name="parameters">The connection parameters to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Creates a protocol handler.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>The protocol handler.</returns>
        IProtocolHandler CreateHandler(Dictionary<string, string> parameters);
    }

    /// <summary>
    /// Represents the capabilities of a protocol.
    /// </summary>
    public class ProtocolCapabilities
    {
        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports reading.
        /// </summary>
        public bool SupportsReading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports writing.
        /// </summary>
        public bool SupportsWriting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports streaming.
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports batching.
        /// </summary>
        public bool SupportsBatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports transactions.
        /// </summary>
        public bool SupportsTransactions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports authentication.
        /// </summary>
        public bool SupportsAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports encryption.
        /// </summary>
        public bool SupportsEncryption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the protocol supports compression.
        /// </summary>
        public bool SupportsCompression { get; set; }

        /// <summary>
        /// Gets or sets the supported data formats.
        /// </summary>
        public List<string> SupportedDataFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported authentication methods.
        /// </summary>
        public List<string> SupportedAuthenticationMethods { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported encryption methods.
        /// </summary>
        public List<string> SupportedEncryptionMethods { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported compression methods.
        /// </summary>
        public List<string> SupportedCompressionMethods { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents the connection parameters for a protocol.
    /// </summary>
    public class ConnectionParameters
    {
        /// <summary>
        /// Gets or sets the required parameters.
        /// </summary>
        public List<ParameterDefinition> RequiredParameters { get; set; } = new List<ParameterDefinition>();

        /// <summary>
        /// Gets or sets the optional parameters.
        /// </summary>
        public List<ParameterDefinition> OptionalParameters { get; set; } = new List<ParameterDefinition>();

        /// <summary>
        /// Gets or sets the authentication parameters.
        /// </summary>
        public List<ParameterDefinition> AuthenticationParameters { get; set; } = new List<ParameterDefinition>();

        /// <summary>
        /// Gets or sets the encryption parameters.
        /// </summary>
        public List<ParameterDefinition> EncryptionParameters { get; set; } = new List<ParameterDefinition>();

        /// <summary>
        /// Gets or sets the compression parameters.
        /// </summary>
        public List<ParameterDefinition> CompressionParameters { get; set; } = new List<ParameterDefinition>();
    }

    /// <summary>
    /// Represents the definition of a parameter.
    /// </summary>
    public class ParameterDefinition
    {
        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameter description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        public ParameterType Type { get; set; } = ParameterType.STRING;

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is sensitive.
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// Gets or sets the validation pattern for the parameter.
        /// </summary>
        public string? ValidationPattern { get; set; }

        /// <summary>
        /// Gets or sets the allowed values for the parameter.
        /// </summary>
        public List<string>? AllowedValues { get; set; }
    }

    /// <summary>
    /// Represents the type of a parameter.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// String parameter type.
        /// </summary>
        STRING,

        /// <summary>
        /// Integer parameter type.
        /// </summary>
        INTEGER,

        /// <summary>
        /// Boolean parameter type.
        /// </summary>
        BOOLEAN,

        /// <summary>
        /// Decimal parameter type.
        /// </summary>
        DECIMAL,

        /// <summary>
        /// Date parameter type.
        /// </summary>
        DATE,

        /// <summary>
        /// Time parameter type.
        /// </summary>
        TIME,

        /// <summary>
        /// DateTime parameter type.
        /// </summary>
        DATETIME,

        /// <summary>
        /// Password parameter type.
        /// </summary>
        PASSWORD,

        /// <summary>
        /// File parameter type.
        /// </summary>
        FILE,

        /// <summary>
        /// Directory parameter type.
        /// </summary>
        DIRECTORY,

        /// <summary>
        /// URL parameter type.
        /// </summary>
        URL,

        /// <summary>
        /// Email parameter type.
        /// </summary>
        EMAIL
    }
}
