using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Integration.Exporters.File.Configuration;
using FlowOrchestrator.Integration.Exporters.File.Models;
using FlowOrchestrator.Integration.Exporters;
using FlowOrchestrator.Integration.Protocols;
using FlowOrchestrator.Integration.Protocols.Protocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Exporters.File
{
    /// <summary>
    /// Service for exporting data to file destinations.
    /// </summary>
    public class FileExporterService : ExporterServiceBase
    {
        private readonly new ILogger<FileExporterService> _logger;
        private readonly new FileExporterConfiguration _configuration;
        private readonly IFileProtocol _fileProtocol;

        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public override string ServiceId => _configuration.ServiceId;

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public override string ServiceType => _configuration.ServiceType;

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Gets the protocol used by this exporter service.
        /// </summary>
        public override string Protocol => "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExporterService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public FileExporterService(
            ILogger<FileExporterService> logger,
            IOptions<FileExporterConfiguration> options)
        {
            _logger = logger;
            _configuration = options.Value;
            _fileProtocol = new FileProtocolAdapter();
        }

        /// <summary>
        /// Gets the capabilities of the exporter service.
        /// </summary>
        /// <returns>The exporter capabilities.</returns>
        public override ExporterCapabilities GetCapabilities()
        {
            var protocolCapabilities = _fileProtocol.GetCapabilities();

            return new ExporterCapabilities
            {
                SupportedDestinationTypes = new List<string> { "file" },
                SupportedDataFormats = protocolCapabilities.SupportedDataFormats,
                SupportedMergeStrategies = new List<MergeStrategy>
                {
                    MergeStrategy.CONCATENATE,
                    MergeStrategy.MERGE,
                    MergeStrategy.LAST_ONLY
                },
                SupportsStreaming = false,
                SupportsBatching = protocolCapabilities.SupportsBatching,
                SupportsTransactions = false,
                MaxBatchSize = _configuration.BatchSize
            };
        }

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public override ValidationResult ValidateParameters(ExportParameters parameters)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(parameters.DestinationId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_ID_REQUIRED", Message = "Destination ID is required." });
            }

            if (string.IsNullOrWhiteSpace(parameters.DestinationLocation))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DESTINATION_LOCATION_REQUIRED", Message = "Destination location is required." });
            }

            if (parameters.Data == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "DATA_REQUIRED", Message = "Data is required." });
            }

            // Validate file path
            var filePath = GetFilePath(parameters);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FILE_PATH_REQUIRED", Message = "File path is required." });
            }

            return result;
        }

        /// <summary>
        /// Gets the merge capabilities of the exporter service.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        public override FlowOrchestrator.Abstractions.Services.MergeCapabilities GetMergeCapabilities()
        {
            return new FlowOrchestrator.Abstractions.Services.MergeCapabilities
            {
                SupportedMergeStrategies = new List<MergeStrategy>
                {
                    MergeStrategy.CONCATENATE,
                    MergeStrategy.MERGE,
                    MergeStrategy.LAST_ONLY
                },
                SupportsCustomMergeStrategies = false,
                MaxBranchCount = 10
            };
        }

        /// <summary>
        /// Merges data from multiple branches.
        /// </summary>
        /// <param name="branchData">The branch data.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        public override ExportResult MergeBranches(Dictionary<string, DataPackage> branchData, MergeStrategy strategy, ExecutionContext context)
        {
            _logger.LogInformation("Merging {Count} branches using strategy: {Strategy}", branchData.Count, strategy);

            // Create a merged data package based on the strategy
            var mergedData = new DataPackage();

            switch (strategy)
            {
                case MergeStrategy.CONCATENATE:
                    // Concatenate all branch data
                    foreach (var branch in branchData)
                    {
                        // In a real implementation, we would concatenate the data
                        // For now, we'll just use the last branch's data
                        mergedData = branch.Value;
                    }
                    break;

                case MergeStrategy.LAST_ONLY:
                    // Use the last branch data
                    if (branchData.Count > 0)
                    {
                        var lastBranch = branchData.Last();
                        mergedData = lastBranch.Value;
                    }
                    break;

                case MergeStrategy.MERGE:
                    // Merge branch data based on keys
                    // This is a simplified implementation
                    foreach (var branch in branchData)
                    {
                        // In a real implementation, we would merge the data
                        // For now, we'll just use the last branch's data
                        mergedData = branch.Value;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported merge strategy: {strategy}");
            }

            // Create the export result
            var result = new ExportResult
            {
                Success = true,
                DestinationMetadata = new Dictionary<string, object>
                {
                    ["BranchCount"] = branchData.Count,
                    ["Strategy"] = strategy.ToString(),
                    ["MergeTime"] = DateTime.UtcNow
                }
            };

            return result;
        }

        /// <summary>
        /// Performs the export operation.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        protected override ExportResult PerformExport(ExportParameters parameters, ExecutionContext context)
        {
            _logger.LogInformation("Exporting data to file destination: {DestinationLocation}", parameters.DestinationLocation);

            try
            {
                // Create protocol handler
                var connectionParams = CreateConnectionParameters(parameters);
                var handler = (FileProtocolHandler)_fileProtocol.CreateHandler(connectionParams);

                // Connect to the destination
                if (!handler.Connect())
                {
                    throw new IOException($"Failed to connect to file destination: {parameters.DestinationLocation}");
                }

                _logger.LogInformation("Connected to file destination: {DestinationLocation}", parameters.DestinationLocation);

                // Create write options
                var writeOptions = CreateWriteOptions(parameters);

                // Write the data
                bool success = handler.Write(parameters.Data, writeOptions);

                if (!success)
                {
                    throw new IOException($"Failed to write data to file destination: {parameters.DestinationLocation}");
                }

                _logger.LogInformation("Successfully wrote data to file destination: {DestinationLocation}", parameters.DestinationLocation);

                // Create configuration parameters for destination information
                var configParams = new ConfigurationParameters();
                configParams.SetParameter("DestinationId", parameters.DestinationId);
                configParams.SetParameter("DestinationType", "file");
                configParams.SetParameter("DestinationLocation", parameters.DestinationLocation);

                // Create the export result
                var result = new ExportResult
                {
                    Success = true,
                    DestinationMetadata = GenerateMetadata(parameters),
                    Destination = new DestinationInformation(configParams)
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to file destination: {DestinationLocation}", parameters.DestinationLocation);
                throw;
            }
        }

        /// <summary>
        /// Creates connection parameters for the file protocol handler.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The connection parameters.</returns>
        private Dictionary<string, string> CreateConnectionParameters(ExportParameters parameters)
        {
            var connectionParams = new Dictionary<string, string>
            {
                ["basePath"] = GetBasePath(parameters),
                ["filePattern"] = _configuration.File.FilePattern,
                ["recursive"] = _configuration.File.Recursive.ToString(),
                ["encoding"] = _configuration.File.Encoding,
                ["bufferSize"] = _configuration.File.BufferSize.ToString()
            };

            return connectionParams;
        }

        /// <summary>
        /// Creates write options for the file protocol handler.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The write options.</returns>
        private Dictionary<string, object> CreateWriteOptions(ExportParameters parameters)
        {
            var filePath = GetFilePath(parameters);
            var options = new Dictionary<string, object>
            {
                ["filePath"] = filePath,
                ["overwrite"] = GetParameterValue(parameters, "overwrite", true),
                ["createDirectory"] = GetParameterValue(parameters, "createDirectory", true),
                ["encoding"] = GetParameterValue(parameters, "encoding", _configuration.File.Encoding)
            };

            return options;
        }

        /// <summary>
        /// Gets the base path for file operations.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The base path.</returns>
        private string GetBasePath(ExportParameters parameters)
        {
            if (parameters.DestinationConfiguration.TryGetValue("basePath", out var basePath) && basePath is string basePathStr)
            {
                return basePathStr;
            }

            return _configuration.File.BasePath;
        }

        /// <summary>
        /// Gets the file path for the export operation.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The file path.</returns>
        private string GetFilePath(ExportParameters parameters)
        {
            if (parameters.DestinationConfiguration.TryGetValue("filePath", out var filePath) && filePath is string filePathStr)
            {
                return filePathStr;
            }

            // If no specific file path is provided, use the destination location
            return parameters.DestinationLocation;
        }

        /// <summary>
        /// Gets a parameter value from the destination configuration.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="key">The parameter key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The parameter value.</returns>
        private T GetParameterValue<T>(ExportParameters parameters, string key, T defaultValue)
        {
            if (parameters.DestinationConfiguration.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    // Ignore conversion errors
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Generates metadata for the export operation.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The metadata.</returns>
        private Dictionary<string, object> GenerateMetadata(ExportParameters parameters)
        {
            var metadata = new Dictionary<string, object>
            {
                ["ExportTime"] = DateTime.UtcNow,
                ["FilePath"] = GetFilePath(parameters),
                ["FileFormat"] = GetFileFormat(parameters)
            };

            // Try to get record count if available
            if (parameters.Data.Metadata.TryGetValue("RecordCount", out var recordCount))
            {
                metadata["RecordCount"] = recordCount;
            }

            return metadata;
        }

        /// <summary>
        /// Gets the file format for the export operation.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The file format.</returns>
        private string GetFileFormat(ExportParameters parameters)
        {
            if (parameters.DestinationConfiguration.TryGetValue("fileFormat", out var fileFormat) && fileFormat is string fileFormatStr)
            {
                return fileFormatStr;
            }

            // Try to determine format from file extension
            var filePath = GetFilePath(parameters);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
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
            /// Gets the connection parameters for the protocol.
            /// </summary>
            /// <returns>The connection parameters.</returns>
            public ConnectionParameters GetConnectionParameters() => _protocol.GetConnectionParameters();

            /// <summary>
            /// Validates connection parameters.
            /// </summary>
            /// <param name="parameters">The connection parameters.</param>
            /// <returns>The validation result.</returns>
            public ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters)
            {
                var result = _protocol.ValidateConnectionParameters(parameters);
                return new ValidationResult
                {
                    IsValid = result.IsValid,
                    Errors = result.Errors.Select(e => new ValidationError
                    {
                        Code = e.Code,
                        Message = e.Message
                    }).ToList()
                };
            }

            /// <summary>
            /// Creates a protocol handler.
            /// </summary>
            /// <param name="parameters">The connection parameters.</param>
            /// <returns>The protocol handler.</returns>
            public IProtocolHandler CreateHandler(Dictionary<string, string> parameters) => _protocol.CreateHandler(parameters);
        }
    }
}
