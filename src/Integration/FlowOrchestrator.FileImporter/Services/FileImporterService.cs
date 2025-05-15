using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Integration.Importers;
using FlowOrchestrator.Integration.Protocols.Protocols;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Importers.File
{
    /// <summary>
    /// Service for importing data from file sources.
    /// </summary>
    public class FileImporterService : ImporterServiceBase
    {
        private readonly IFileProtocol _fileProtocol;
        private readonly new ILogger<FileImporterService> _logger;

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => "FILE-IMPORTER-001";

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => "FileImporter";

        /// <summary>
        /// Gets the protocol used by this importer.
        /// </summary>
        public override string Protocol => "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImporterService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FileImporterService(ILogger<FileImporterService> logger) : base(new LoggerAdapter(logger))
        {
            _logger = logger;
            _fileProtocol = new FileProtocolAdapter();
        }

        /// <summary>
        /// Adapter class to implement ILogger using Microsoft.Extensions.Logging.ILogger.
        /// </summary>
        private class LoggerAdapter : FlowOrchestrator.Integration.Importers.ILogger
        {
            private readonly Microsoft.Extensions.Logging.ILogger _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="LoggerAdapter"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public LoggerAdapter(Microsoft.Extensions.Logging.ILogger logger)
            {
                _logger = logger;
            }

            /// <summary>
            /// Logs an informational message.
            /// </summary>
            /// <param name="message">The message to log.</param>
            public void LogInformation(string message)
            {
                _logger.LogInformation(message);
            }

            /// <summary>
            /// Logs a warning message.
            /// </summary>
            /// <param name="message">The message to log.</param>
            public void LogWarning(string message)
            {
                _logger.LogWarning(message);
            }

            /// <summary>
            /// Logs an error message.
            /// </summary>
            /// <param name="ex">The exception that caused the error.</param>
            /// <param name="message">The message to log.</param>
            public void LogError(Exception ex, string message)
            {
                _logger.LogError(ex, message);
            }
        }

        /// <summary>
        /// Adapter class to implement IFileProtocol using FileProtocol.
        /// </summary>
        private class FileProtocolAdapter : IFileProtocol
        {
            private readonly FileProtocol _protocol = new FileProtocol();

            /// <summary>
            /// Gets the protocol identifier.
            /// </summary>
            public string ProtocolId => "file";

            /// <summary>
            /// Gets the protocol name.
            /// </summary>
            public string Name => _protocol.Name;

            /// <summary>
            /// Gets the protocol version.
            /// </summary>
            public string Version => "1.0.0";

            /// <summary>
            /// Gets the capabilities of the protocol.
            /// </summary>
            /// <returns>The protocol capabilities.</returns>
            public ProtocolCapabilities GetCapabilities() => _protocol.GetCapabilities();

            /// <summary>
            /// Creates a protocol handler.
            /// </summary>
            /// <param name="connectionParameters">The connection parameters.</param>
            /// <returns>The protocol handler.</returns>
            public IProtocolHandler CreateHandler(Dictionary<string, string> connectionParameters) =>
                _protocol.CreateHandler(connectionParameters);
        }

        /// <summary>
        /// Gets the capabilities of the importer service.
        /// </summary>
        /// <returns>The importer capabilities.</returns>
        public override ImporterCapabilities GetCapabilities()
        {
            var protocolCapabilities = _fileProtocol.GetCapabilities();

            return new ImporterCapabilities
            {
                SupportedSourceTypes = new List<string> { "file" },
                SupportedDataFormats = protocolCapabilities.SupportedDataFormats,
                SupportsValidation = true,
                SupportsStreaming = false,
                SupportsBatching = true,
                SupportsFiltering = true,
                SupportsTransformation = false,
                MaxBatchSize = 100
            };
        }

        /// <summary>
        /// Performs the import operation.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        protected override ImportResult PerformImport(ImportParameters parameters, ExecutionContext context)
        {
            _logger.LogInformation("Starting file import operation for source: {SourceId}", parameters.SourceId);

            try
            {
                // Create connection parameters for the file protocol
                var connectionParams = CreateConnectionParameters(parameters);

                // Create a protocol handler
                using var handler = _fileProtocol.CreateHandler(connectionParams);

                // Connect to the source
                if (!handler.Connect())
                {
                    throw new InvalidOperationException($"Failed to connect to file source: {parameters.SourceLocation}");
                }

                _logger.LogInformation("Connected to file source: {SourceLocation}", parameters.SourceLocation);

                // Create read options
                var readOptions = CreateReadOptions(parameters);

                // Read the data
                var dataPackage = handler.Read(readOptions);

                _logger.LogInformation("Successfully read data from file source: {SourceLocation}", parameters.SourceLocation);

                // Create the import result
                var result = new ImportResult
                {
                    Success = true,
                    Data = dataPackage,
                    Source = dataPackage.Source,
                    SourceMetadata = GenerateMetadata(parameters, dataPackage)
                };

                // Validate the data if a schema is provided
                if (!string.IsNullOrEmpty(parameters.SchemaId))
                {
                    result.ValidationResults = ValidateData(dataPackage, parameters.SchemaId);
                    result.Success = result.ValidationResults.IsValid;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file import operation: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates connection parameters for the file protocol.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <returns>The connection parameters.</returns>
        private Dictionary<string, string> CreateConnectionParameters(ImportParameters parameters)
        {
            var connectionParams = new Dictionary<string, string>();

            // Extract base path from source location or configuration
            string basePath = parameters.SourceLocation;
            if (string.IsNullOrEmpty(basePath) && parameters.SourceConfiguration.TryGetValue("basePath", out var basePathObj))
            {
                basePath = basePathObj?.ToString() ?? string.Empty;
            }

            connectionParams["basePath"] = basePath;

            // Extract file pattern from configuration
            if (parameters.SourceConfiguration.TryGetValue("filePattern", out var filePatternObj))
            {
                connectionParams["filePattern"] = filePatternObj?.ToString() ?? "*.*";
            }
            else
            {
                connectionParams["filePattern"] = "*.*";
            }

            // Extract recursive flag from configuration
            if (parameters.SourceConfiguration.TryGetValue("recursive", out var recursiveObj))
            {
                connectionParams["recursive"] = recursiveObj?.ToString() ?? "false";
            }
            else
            {
                connectionParams["recursive"] = "false";
            }

            // Extract encoding from configuration
            if (parameters.SourceConfiguration.TryGetValue("encoding", out var encodingObj))
            {
                connectionParams["encoding"] = encodingObj?.ToString() ?? "utf-8";
            }
            else
            {
                connectionParams["encoding"] = "utf-8";
            }

            return connectionParams;
        }

        /// <summary>
        /// Creates read options for the file protocol.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <returns>The read options.</returns>
        private Dictionary<string, object> CreateReadOptions(ImportParameters parameters)
        {
            var readOptions = new Dictionary<string, object>();

            // Extract file path from configuration
            if (parameters.SourceConfiguration.TryGetValue("filePath", out var filePathObj))
            {
                readOptions["filePath"] = filePathObj ?? string.Empty;
            }

            return readOptions;
        }

        /// <summary>
        /// Generates metadata for the import result.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="dataPackage">The data package.</param>
        /// <returns>The metadata.</returns>
        private Dictionary<string, object> GenerateMetadata(ImportParameters parameters, DataPackage dataPackage)
        {
            var metadata = new Dictionary<string, object>
            {
                { "ImportTime", DateTime.UtcNow },
                { "SourceId", parameters.SourceId },
                { "SourceType", "file" },
                { "SourceLocation", parameters.SourceLocation },
                { "ContentType", dataPackage.ContentType }
            };

            // Add file-specific metadata
            if (dataPackage.Source != null && !string.IsNullOrEmpty(dataPackage.Source.SourceLocation))
            {
                var fileInfo = new FileInfo(dataPackage.Source.SourceLocation);
                if (fileInfo.Exists)
                {
                    metadata["FileName"] = fileInfo.Name;
                    metadata["FileSize"] = fileInfo.Length;
                    metadata["FileCreationTime"] = fileInfo.CreationTimeUtc;
                    metadata["FileLastModifiedTime"] = fileInfo.LastWriteTimeUtc;
                }
            }

            return metadata;
        }

        /// <summary>
        /// Validates the data against a schema.
        /// </summary>
        /// <param name="dataPackage">The data package to validate.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns>The validation result.</returns>
        private ValidationResult ValidateData(DataPackage dataPackage, string schemaId)
        {
            // In a real implementation, this would use a schema registry to retrieve the schema
            // and validate the data against it. For now, we'll just return a success result.
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected override string ClassifyException(Exception ex)
        {
            return ex switch
            {
                FileNotFoundException => "FILE_NOT_FOUND",
                DirectoryNotFoundException => "DIRECTORY_NOT_FOUND",
                IOException => "IO_ERROR",
                UnauthorizedAccessException => "ACCESS_DENIED",
                JsonException => "INVALID_JSON_FORMAT",
                InvalidOperationException => "INVALID_OPERATION",
                ArgumentException => "INVALID_ARGUMENT",
                _ => "GENERAL_ERROR"
            };
        }

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
        protected override Dictionary<string, object> GetErrorDetails(Exception ex)
        {
            var details = new Dictionary<string, object>
            {
                { "message", ex.Message },
                { "stackTrace", ex.StackTrace ?? string.Empty },
                { "source", ex.Source ?? string.Empty },
                { "type", ex.GetType().Name }
            };

            if (ex is FileNotFoundException fileNotFoundEx)
            {
                details["fileName"] = fileNotFoundEx.FileName ?? string.Empty;
            }

            return details;
        }
    }
}
