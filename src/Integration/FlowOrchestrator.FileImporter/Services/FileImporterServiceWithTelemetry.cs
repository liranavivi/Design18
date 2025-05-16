using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Protocols;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Abstractions.Statistics;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging;
using FlowOrchestrator.Integration.Importers;
using FlowOrchestrator.Integration.Protocols.Protocols;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Importers.File
{
    /// <summary>
    /// Service for importing data from file sources with OpenTelemetry integration.
    /// </summary>
    public class FileImporterServiceWithTelemetry : AbstractServiceBase, IImporterService
    {
        private readonly FileProtocol _fileProtocol;
        private readonly LoggingService _loggingService;

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
        public string Protocol => "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImporterServiceWithTelemetry"/> class.
        /// </summary>
        /// <param name="statisticsProvider">The statistics provider.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="loggingService">The logging service.</param>
        public FileImporterServiceWithTelemetry(
            IStatisticsProvider statisticsProvider,
            ILogger<FileImporterServiceWithTelemetry> logger,
            LoggingService loggingService)
            : base(statisticsProvider, logger)
        {
            _loggingService = loggingService;
            _fileProtocol = new FileProtocol();
        }

        /// <summary>
        /// Gets the capabilities of the importer service.
        /// </summary>
        /// <returns>The importer capabilities.</returns>
        public ImporterCapabilities GetCapabilities()
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
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>The validation result.</returns>
        public ValidationResult ValidateParameters(ImportParameters parameters)
        {
            _loggingService.LogInformation("Validating import parameters for source: {SourceId}", parameters.SourceId);

            var result = new ValidationResult { IsValid = true };

            // Validate source ID
            if (string.IsNullOrEmpty(parameters.SourceId))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "MISSING_SOURCE_ID", Message = "SourceId is required" });
            }

            // Validate source location
            if (string.IsNullOrEmpty(parameters.SourceLocation) &&
                (!parameters.SourceConfiguration.TryGetValue("basePath", out var basePathObj) ||
                 string.IsNullOrEmpty(basePathObj?.ToString())))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "MISSING_SOURCE_LOCATION", Message = "Source location is required either in SourceLocation or in SourceConfiguration.basePath" });
            }

            // Log validation results
            if (result.IsValid)
            {
                _loggingService.LogInformation("Import parameters validation successful for source: {SourceId}", parameters.SourceId);
            }
            else
            {
                _loggingService.LogWarning("Import parameters validation failed for source: {SourceId}. Errors: {Errors}",
                    parameters.SourceId, string.Join(", ", result.Errors));
            }

            return result;
        }

        /// <summary>
        /// Imports data from a source.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        public ImportResult Import(ImportParameters parameters, ExecutionContext context)
        {
            if (GetState() != ServiceState.READY)
            {
                throw new InvalidOperationException($"Service not in READY state. Current state: {GetState()}");
            }

            SetState(ServiceState.PROCESSING);
            var result = new ImportResult();

            try
            {
                _statisticsProvider.StartOperation("file.import");
                _loggingService.LogInformation("Starting file import operation for source: {SourceId}", parameters.SourceId);

                // Perform the import
                result = PerformImport(parameters, context);

                // Record metrics
                if (result.Success)
                {
                    _statisticsProvider.IncrementCounter("file.import.success");
                    _statisticsProvider.EndOperation("file.import", OperationResult.SUCCESS);
                    _loggingService.LogInformation("File import operation completed successfully for source: {SourceId}", parameters.SourceId);
                }
                else
                {
                    _statisticsProvider.IncrementCounter("file.import.failure");
                    _statisticsProvider.EndOperation("file.import", OperationResult.FAILURE);
                    _loggingService.LogWarning("File import operation failed for source: {SourceId}", parameters.SourceId);
                }
            }
            catch (Exception ex)
            {
                _statisticsProvider.IncrementCounter("file.import.error");
                _statisticsProvider.RecordMetric("file.import.error.type", ClassifyException(ex));
                _statisticsProvider.EndOperation("file.import", OperationResult.FAILURE);

                _loggingService.LogError(ex, "Error during file import operation for source: {SourceId}", parameters.SourceId);

                result.Success = false;
                result.Error = new ExecutionError
                {
                    ErrorCode = ClassifyException(ex),
                    ErrorMessage = ex.Message,
                    ErrorDetails = GetErrorDetails(ex)
                };
            }
            finally
            {
                SetState(ServiceState.READY);
            }

            return result;
        }

        /// <summary>
        /// Performs the import operation.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        protected ImportResult PerformImport(ImportParameters parameters, ExecutionContext context)
        {
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

                _loggingService.LogInformation("Connected to file source: {SourceLocation}", parameters.SourceLocation);

                // Create read options
                var readOptions = CreateReadOptions(parameters);

                // Read the data
                _statisticsProvider.StartOperation("file.read");
                var dataPackage = handler.Read(readOptions);
                _statisticsProvider.EndOperation("file.read", OperationResult.SUCCESS);

                // Record metrics about the data
                if (dataPackage != null)
                {
                    _statisticsProvider.RecordMetric("file.data.size", 1); // Use a placeholder value since we don't have direct access to data length
                }

                _loggingService.LogInformation("Successfully read data from file source: {SourceLocation}", parameters.SourceLocation);

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
                    _statisticsProvider.StartOperation("file.validate");
                    result.ValidationResults = ValidateData(dataPackage, parameters.SchemaId);
                    result.Success = result.ValidationResults.IsValid;

                    var validationResult = result.ValidationResults.IsValid ?
                        OperationResult.SUCCESS : OperationResult.FAILURE;
                    _statisticsProvider.EndOperation("file.validate", validationResult);

                    if (!result.ValidationResults.IsValid)
                    {
                        _loggingService.LogWarning("Data validation failed for file source: {SourceLocation}",
                            parameters.SourceLocation);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error during file import operation: {ErrorMessage}", ex.Message);
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
        /// Validates the data against a schema.
        /// </summary>
        /// <param name="dataPackage">The data package to validate.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns>The validation result.</returns>
        private ValidationResult ValidateData(DataPackage dataPackage, string schemaId)
        {
            // In a real implementation, this would validate the data against a schema
            // For now, we'll just return a success result
            return new ValidationResult { IsValid = true };
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
                { "SourceLocation", parameters.SourceLocation }
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
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected string ClassifyException(Exception ex)
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
        protected Dictionary<string, object> GetErrorDetails(Exception ex)
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

        /// <summary>
        /// Validates the configuration parameters.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateConfiguration(ConfigurationParameters parameters)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate required parameters
            if (!parameters.ContainsKey("ServiceId"))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "MISSING_SERVICE_ID", Message = "ServiceId is required" });
            }

            return result;
        }

        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        protected override void OnInitialize(ConfigurationParameters parameters)
        {
            // No additional initialization needed
        }

        /// <summary>
        /// Called when the service is ready.
        /// </summary>
        protected override void OnReady()
        {
            _loggingService.LogInformation("File importer service is ready");
        }

        /// <summary>
        /// Called when the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        protected override void OnError(Exception ex)
        {
            _loggingService.LogError(ex, "File importer service encountered an error");
        }

        /// <summary>
        /// Called when the service is being terminated.
        /// </summary>
        protected override void OnTerminate()
        {
            _loggingService.LogInformation("File importer service is terminating");
        }
    }
}
