using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Logging;
using FlowOrchestrator.Integration.Importers.File;
using FlowOrchestrator.Integration.Importers.File.Utilities;

namespace FlowOrchestrator.FileImporter
{
    /// <summary>
    /// Worker service for the FileImporter.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FileImporterServiceWithTelemetry _fileImporterService;
        private readonly IConfiguration _configuration;
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="fileImporterService">The file importer service.</param>
        /// <param name="configuration">The configuration.</param>
        public Worker(
            ILogger<Worker> logger,
            FileImporterServiceWithTelemetry fileImporterService,
            IConfiguration configuration,
            LoggingService loggingService)
        {
            _logger = logger;
            _fileImporterService = fileImporterService;
            _configuration = configuration;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Executes the worker service.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _loggingService.LogInformation("FileImporter worker service starting at: {Time}", DateTimeOffset.Now);

            try
            {
                // Initialize the file importer service
                var configParams = LoadConfiguration();
                _fileImporterService.Initialize(configParams);

                _loggingService.LogInformation("FileImporter service initialized successfully");

                // Main service loop
                while (!stoppingToken.IsCancellationRequested)
                {
                    _loggingService.LogDebug("FileImporter worker running at: {Time}", DateTimeOffset.Now);

                    // In a real implementation, this would listen for import commands
                    // from a message queue or other source. For now, we'll just wait.

                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error in FileImporter worker service: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                // Terminate the file importer service
                _fileImporterService.Terminate();
                _loggingService.LogInformation("FileImporter service terminated");
            }
        }

        /// <summary>
        /// Loads the configuration for the file importer service.
        /// </summary>
        /// <returns>The configuration parameters.</returns>
        private ConfigurationParameters LoadConfiguration()
        {
            var configParams = new ConfigurationParameters();

            // Load configuration from appsettings.json
            var fileImporterSection = _configuration.GetSection("FileImporter");
            if (fileImporterSection.Exists())
            {
                // Add general service configuration
                configParams.SetParameter("ServiceId", fileImporterSection["ServiceId"] ?? "FILE-IMPORTER-001");
                configParams.SetParameter("ServiceType", fileImporterSection["ServiceType"] ?? "FileImporter");
                configParams.SetParameter("Protocol", fileImporterSection["Protocol"] ?? "file");

                // Add timeout configuration
                if (int.TryParse(fileImporterSection["ConnectionTimeoutSeconds"], out int connectionTimeout))
                {
                    configParams.SetParameter("ConnectionTimeoutSeconds", connectionTimeout);
                }
                else
                {
                    configParams.SetParameter("ConnectionTimeoutSeconds", 30);
                }

                if (int.TryParse(fileImporterSection["OperationTimeoutSeconds"], out int operationTimeout))
                {
                    configParams.SetParameter("OperationTimeoutSeconds", operationTimeout);
                }
                else
                {
                    configParams.SetParameter("OperationTimeoutSeconds", 60);
                }

                // Add retry configuration
                if (int.TryParse(fileImporterSection["MaxRetryCount"], out int maxRetryCount))
                {
                    configParams.SetParameter("MaxRetryCount", maxRetryCount);
                }
                else
                {
                    configParams.SetParameter("MaxRetryCount", 3);
                }

                if (int.TryParse(fileImporterSection["RetryDelayMilliseconds"], out int retryDelay))
                {
                    configParams.SetParameter("RetryDelayMilliseconds", retryDelay);
                }
                else
                {
                    configParams.SetParameter("RetryDelayMilliseconds", 1000);
                }

                // Add file-specific configuration
                var fileSection = fileImporterSection.GetSection("File");
                if (fileSection.Exists())
                {
                    configParams.SetParameter("BasePath", fileSection["BasePath"] ?? string.Empty);
                    configParams.SetParameter("FilePattern", fileSection["FilePattern"] ?? "*.*");
                    configParams.SetParameter("Recursive", fileSection["Recursive"] ?? "false");
                    configParams.SetParameter("Encoding", fileSection["Encoding"] ?? "utf-8");
                }
            }
            else
            {
                // Use default configuration
                configParams.SetParameter("ServiceId", "FILE-IMPORTER-001");
                configParams.SetParameter("ServiceType", "FileImporter");
                configParams.SetParameter("Protocol", "file");
                configParams.SetParameter("ConnectionTimeoutSeconds", 30);
                configParams.SetParameter("OperationTimeoutSeconds", 60);
                configParams.SetParameter("MaxRetryCount", 3);
                configParams.SetParameter("RetryDelayMilliseconds", 1000);
                configParams.SetParameter("BasePath", string.Empty);
                configParams.SetParameter("FilePattern", "*.*");
                configParams.SetParameter("Recursive", "false");
                configParams.SetParameter("Encoding", "utf-8");
            }

            return configParams;
        }
    }
}
