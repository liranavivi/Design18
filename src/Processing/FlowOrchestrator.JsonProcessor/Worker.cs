using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Processing.Json.Services;

namespace FlowOrchestrator.Processing.Json
{
    /// <summary>
    /// Worker service for the JSON processor.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly JsonProcessorService _jsonProcessorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="jsonProcessorService">The JSON processor service.</param>
        public Worker(
            ILogger<Worker> logger,
            JsonProcessorService jsonProcessorService)
        {
            _logger = logger;
            _jsonProcessorService = jsonProcessorService;
        }

        /// <summary>
        /// Executes the worker service.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JSON Processor Service starting at: {time}", DateTimeOffset.Now);

            try
            {
                // Initialize the JSON processor service
                var configParams = new ConfigurationParameters();
                configParams.SetParameter("ServiceId", "JSON-PROCESSOR-001");
                configParams.SetParameter("MaxDepth", 64);
                configParams.SetParameter("MaxStringLength", 1048576);
                _jsonProcessorService.Initialize(configParams);
                _logger.LogInformation("JSON Processor Service initialized successfully");

                // Main service loop
                while (!stoppingToken.IsCancellationRequested)
                {
                    // In a real implementation, this would listen for process commands
                    // from a message bus or other source

                    // For now, we'll just wait for a short period
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JSON Processor Service: {ErrorMessage}", ex.Message);
            }
            finally
            {
                // Terminate the processor service
                _jsonProcessorService.Terminate();
                _logger.LogInformation("JSON Processor Service terminated at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
