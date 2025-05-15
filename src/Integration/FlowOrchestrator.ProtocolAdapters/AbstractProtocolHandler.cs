using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;

namespace FlowOrchestrator.Integration.Protocols
{
    /// <summary>
    /// Abstract base class for all protocol handlers in the FlowOrchestrator system.
    /// Provides common functionality and a standardized implementation pattern.
    /// </summary>
    public abstract class AbstractProtocolHandler : IProtocolHandler
    {
        private bool _disposed = false;

        /// <summary>
        /// Gets the protocol name.
        /// </summary>
        public abstract string ProtocolName { get; }

        /// <summary>
        /// Gets a value indicating whether the handler is connected.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets the connection parameters.
        /// </summary>
        public Dictionary<string, string> ConnectionParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProtocolHandler"/> class.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        protected AbstractProtocolHandler(Dictionary<string, string> parameters)
        {
            ConnectionParameters = parameters ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <returns>true if the connection was opened successfully; otherwise, false.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Reads data from the connection.
        /// </summary>
        /// <param name="options">The read options.</param>
        /// <returns>The data read from the connection.</returns>
        public abstract DataPackage Read(Dictionary<string, object> options);

        /// <summary>
        /// Writes data to the connection.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="options">The write options.</param>
        /// <returns>true if the data was written successfully; otherwise, false.</returns>
        public abstract bool Write(DataPackage data, Dictionary<string, object> options);

        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <returns>The connection test result.</returns>
        public virtual ConnectionTestResult TestConnection()
        {
            try
            {
                if (!IsConnected)
                {
                    bool connected = Connect();
                    if (connected)
                    {
                        return new ConnectionTestResult
                        {
                            Success = true,
                            ConnectionInfo = new Dictionary<string, object>
                            {
                                { "Status", "Connection successful" }
                            }
                        };
                    }
                    else
                    {
                        return new ConnectionTestResult
                        {
                            Success = false,
                            ErrorMessage = "Failed to connect",
                            ConnectionInfo = new Dictionary<string, object>
                            {
                                { "Status", "Failed" }
                            }
                        };
                    }
                }
                return new ConnectionTestResult
                {
                    Success = true,
                    ConnectionInfo = new Dictionary<string, object>
                    {
                        { "Status", "Already connected" }
                    }
                };
            }
            catch (Exception ex)
            {
                var errorDetails = GetErrorDetails(ex);

                return new ConnectionTestResult
                {
                    Success = false,
                    ErrorMessage = $"Connection test failed: {ex.Message}",
                    ErrorDetails = errorDetails,
                    ConnectionInfo = new Dictionary<string, object>
                    {
                        { "Status", "Error" }
                    }
                };
            }
        }

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <returns>true if the transaction was started successfully; otherwise, false.</returns>
        public virtual bool BeginTransaction()
        {
            // Base implementation does not support transactions
            return false;
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <returns>true if the transaction was committed successfully; otherwise, false.</returns>
        public virtual bool CommitTransaction()
        {
            // Base implementation does not support transactions
            return false;
        }

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        /// <returns>true if the transaction was rolled back successfully; otherwise, false.</returns>
        public virtual bool RollbackTransaction()
        {
            // Base implementation does not support transactions
            return false;
        }

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected abstract string ClassifyException(Exception ex);

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
        protected abstract Dictionary<string, object> GetErrorDetails(Exception ex);

        /// <summary>
        /// Validates the read options.
        /// </summary>
        /// <param name="options">The read options to validate.</param>
        /// <returns>true if the options are valid; otherwise, false.</returns>
        protected virtual bool ValidateReadOptions(Dictionary<string, object> options)
        {
            return options != null;
        }

        /// <summary>
        /// Validates the write options.
        /// </summary>
        /// <param name="options">The write options to validate.</param>
        /// <returns>true if the options are valid; otherwise, false.</returns>
        protected virtual bool ValidateWriteOptions(Dictionary<string, object> options)
        {
            return options != null;
        }

        /// <summary>
        /// Gets a parameter value from the connection parameters.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found or cannot be converted.</param>
        /// <returns>The parameter value if found and convertible; otherwise, the default value.</returns>
        protected T GetParameterValue<T>(string paramName, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(paramName) || !ConnectionParameters.TryGetValue(paramName, out var stringValue))
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

        /// <summary>
        /// Gets an option value from the options dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the option value.</typeparam>
        /// <param name="options">The options dictionary.</param>
        /// <param name="optionName">The option name.</param>
        /// <param name="defaultValue">The default value to return if the option is not found or cannot be converted.</param>
        /// <returns>The option value if found and convertible; otherwise, the default value.</returns>
        protected T GetOptionValue<T>(Dictionary<string, object> options, string optionName, T defaultValue)
        {
            if (options == null || string.IsNullOrWhiteSpace(optionName) || !options.TryGetValue(optionName, out var value))
            {
                return defaultValue;
            }

            try
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                else
                {
                    // Try to convert the value to the requested type
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Disposes the protocol handler.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the protocol handler.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Disconnect if connected
                    if (IsConnected)
                    {
                        try
                        {
                            Disconnect();
                        }
                        catch
                        {
                            // Ignore exceptions during disposal
                        }
                    }

                    // Dispose any managed resources
                    DisposeManagedResources();
                }

                // Dispose any unmanaged resources
                DisposeUnmanagedResources();

                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes any managed resources.
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Disposes any unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AbstractProtocolHandler"/> class.
        /// </summary>
        ~AbstractProtocolHandler()
        {
            Dispose(false);
        }
    }
}
