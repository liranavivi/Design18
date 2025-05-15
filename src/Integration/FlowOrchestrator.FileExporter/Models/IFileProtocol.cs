using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;

namespace FlowOrchestrator.Integration.Exporters.File.Models
{
    /// <summary>
    /// Interface for file protocol operations.
    /// </summary>
    public interface IFileProtocol
    {
        /// <summary>
        /// Gets the protocol identifier.
        /// </summary>
        string ProtocolId { get; }

        /// <summary>
        /// Gets the protocol name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the protocol version.
        /// </summary>
        string Version { get; }

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
        /// Validates connection parameters.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Creates a protocol handler.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        /// <returns>The protocol handler.</returns>
        IProtocolHandler CreateHandler(Dictionary<string, string> parameters);
    }
}
