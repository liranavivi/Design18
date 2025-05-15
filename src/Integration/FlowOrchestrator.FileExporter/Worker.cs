using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Integration.Exporters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOrchestrator.Integration.Exporters.File
{
    /// <summary>
    /// Worker service for the file exporter.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FileExporterService _exporterService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exporterService">The file exporter service.</param>
        public Worker(
            ILogger<Worker> logger,
            FileExporterService exporterService)
        {
            _logger = logger;
            _exporterService = exporterService;
        }

        /// <summary>
        /// Executes the worker service.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Exporter Service starting at: {time}", DateTimeOffset.Now);

            try
            {
                // Initialize the exporter service
                _exporterService.Initialize(new ConfigurationParameters());
                _logger.LogInformation("File Exporter Service initialized successfully");

                // Main service loop
                while (!stoppingToken.IsCancellationRequested)
                {
                    // In a real implementation, this would listen for export commands
                    // from a message bus or other source

                    // For now, we'll just wait for a short period
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in File Exporter Service");
            }
            finally
            {
                // Terminate the exporter service
                _exporterService.Terminate();
                _logger.LogInformation("File Exporter Service terminated at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
