using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Protocols
{
    /// <summary>
    /// Defines the interface for protocol handlers in the FlowOrchestrator system.
    /// </summary>
    public interface IProtocolHandler : IDisposable
    {
        /// <summary>
        /// Gets the protocol name.
        /// </summary>
        string ProtocolName { get; }

        /// <summary>
        /// Gets a value indicating whether the handler is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the connection parameters.
        /// </summary>
        Dictionary<string, string> ConnectionParameters { get; }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <returns>true if the connection was opened successfully; otherwise, false.</returns>
        bool Connect();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Reads data from the connection.
        /// </summary>
        /// <param name="options">The read options.</param>
        /// <returns>The data read from the connection.</returns>
        DataPackage Read(Dictionary<string, object> options);

        /// <summary>
        /// Writes data to the connection.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="options">The write options.</param>
        /// <returns>true if the data was written successfully; otherwise, false.</returns>
        bool Write(DataPackage data, Dictionary<string, object> options);

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <returns>true if the transaction was started successfully; otherwise, false.</returns>
        bool BeginTransaction();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <returns>true if the transaction was committed successfully; otherwise, false.</returns>
        bool CommitTransaction();

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        /// <returns>true if the transaction was rolled back successfully; otherwise, false.</returns>
        bool RollbackTransaction();

        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <returns>The test result.</returns>
        ConnectionTestResult TestConnection();
    }

    /// <summary>
    /// Represents the result of a connection test.
    /// </summary>
    public class ConnectionTestResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the test was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the test failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error details if the test failed.
        /// </summary>
        public Dictionary<string, object>? ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets the connection information.
        /// </summary>
        public Dictionary<string, object> ConnectionInfo { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the test metrics.
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
    }
}
