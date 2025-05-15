using FlowOrchestrator.Abstractions.Protocols;
using FlowOrchestrator.Integration.Protocols.Protocols;

namespace FlowOrchestrator.Integration.Importers.File
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
        /// Creates a protocol handler.
        /// </summary>
        /// <param name="connectionParameters">The connection parameters.</param>
        /// <returns>The protocol handler.</returns>
        IProtocolHandler CreateHandler(Dictionary<string, string> connectionParameters);
    }
}
