using Shared.Processor.Models;
using Shared.Processor.Services;
using Shared.Entities.Base;
using Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Shared.Processor.Application;

/// <summary>
/// Abstract base class for processor applications
/// </summary>
public abstract class BaseProcessorApplication : IActivityExecutor
{
    private IHost? _host;
    private ILogger<BaseProcessorApplication>? _logger;
    private ProcessorConfiguration? _config;

    /// <summary>
    /// Protected property to access the service provider for derived classes
    /// </summary>
    protected IServiceProvider ServiceProvider => _host?.Services ?? throw new InvalidOperationException("Host not initialized");

    /// <summary>
    /// Main implementation of activity execution that handles common patterns
    /// </summary>
    public virtual async Task<string> ExecuteActivityAsync(
        Guid processorId,
        Guid orchestratedFlowEntityId,
        Guid stepId,
        Guid executionId,
        List<BaseEntity> entities,
        string inputData,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<BaseProcessorApplication>>();

        // Simulate some processing time
        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

        // Parse and validate input data
        JsonElement inputObject;
        JsonElement inputDataObj;
        JsonElement? inputMetadata = null;

        if (string.IsNullOrEmpty(inputData))
        {
            // Create default empty structures for empty input
            var emptyJson = "{\"data\":{},\"metadata\":{}}";
            inputObject = JsonSerializer.Deserialize<JsonElement>(emptyJson);
            inputDataObj = inputObject.GetProperty("data");
        }
        else
        {
            // Parse input data for normal case
            inputObject = JsonSerializer.Deserialize<JsonElement>(inputData);
            inputDataObj = inputObject.GetProperty("data");
            inputMetadata = inputObject.TryGetProperty("metadata", out var metadataElement) ? metadataElement : null;
        }

        logger.LogInformation(
            "Processing input data with {EntitiesInInput} entities from input data, {EntitiesInMessage} entities from message",
            inputDataObj.TryGetProperty("entities", out var entitiesElement) ? entitiesElement.GetArrayLength() : 0,
            entities.Count);

        // Call the abstract method that derived classes must implement
        var processedData = await ProcessActivityDataAsync(
            processorId,
            orchestratedFlowEntityId,
            stepId,
            executionId,
            entities,
            inputDataObj,
            inputMetadata,
            correlationId,
            cancellationToken);

        // Set the executionId of the processed data
        executionId = processedData.ExecutionId;

        // Create standard result structure
        var result = new
        {
            result = processedData.Result ?? "Processing completed successfully",
            timestamp = DateTime.UtcNow.ToString("O"),
            stepId = stepId.ToString(),
            executionId = executionId.ToString(),
            correlationId = correlationId,
            status = processedData.Status ?? "completed",
            data = processedData.Data ?? new { },
            metadata = new
            {
                processor = processedData.ProcessorName ?? GetType().Name,
                version = processedData.Version ?? "1.0",
                environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development",
                machineName = Environment.MachineName
            }
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Abstract method that concrete processor implementations must override
    /// This is where the specific processor business logic should be implemented
    /// </summary>
    /// <param name="processorId">ID of the processor executing the activity</param>
    /// <param name="orchestratedFlowEntityId">ID of the orchestrated flow entity</param>
    /// <param name="stepId">ID of the step being executed</param>
    /// <param name="executionId">Unique execution ID for this activity instance</param>
    /// <param name="entities">Collection of base entities to process</param>
    /// <param name="inputData">Parsed input data object</param>
    /// <param name="inputMetadata">Optional metadata from input</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Processed data that will be incorporated into the standard result structure</returns>
    protected abstract Task<ProcessedActivityData> ProcessActivityDataAsync(
        Guid processorId,
        Guid orchestratedFlowEntityId,
        Guid stepId,
        Guid executionId,
        List<BaseEntity> entities,
        JsonElement inputData,
        JsonElement? inputMetadata,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Data structure for returning processed activity data
    /// </summary>
    protected class ProcessedActivityData
    {
        public string? Result { get; set; }
        public string? Status { get; set; }
        public object? Data { get; set; }
        public string? ProcessorName { get; set; }
        public string? Version { get; set; }
        public Guid ExecutionId { get; set; }
    }

    /// <summary>
    /// Main entry point for the processor application
    /// Sets up infrastructure and starts the application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code (0 for success, non-zero for failure)</returns>
    public async Task<int> RunAsync(string[] args)
    {
        // Initialize console output and display startup information
        var (processorName, processorVersion) = await InitializeConsoleAndDisplayStartupInfoAsync();

        try
        {
            Console.WriteLine($"🔧 Initializing {processorName} Application...");

            // Build and configure the host (following EntitiesManager.Api pattern)
            _host = CreateHostBuilder(args).Build();

            // Get logger and configuration from DI container
            _logger = _host.Services.GetRequiredService<ILogger<BaseProcessorApplication>>();
            _config = _host.Services.GetRequiredService<IOptions<ProcessorConfiguration>>().Value;

            _logger.LogInformation("Starting {ApplicationName}", GetType().Name);

            _logger.LogInformation(
                "Initializing {ApplicationName} - {ProcessorName} v{ProcessorVersion}",
                GetType().Name, _config.Name, _config.Version);

            _logger.LogInformation("Starting host services (MassTransit, Hazelcast, etc.)...");

            // Start the host first (this will start MassTransit consumers)
            await _host.StartAsync();

            _logger.LogInformation("Host services started successfully. Now initializing processor...");

            // Initialize the processor service AFTER host is started
            var processorService = _host.Services.GetRequiredService<IProcessorService>();
            var initializationConfig = _host.Services.GetRequiredService<IOptions<ProcessorInitializationConfiguration>>().Value;

            if (initializationConfig.RetryEndlessly)
            {
                // Start initialization in background - don't wait for completion
                var appLifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await processorService.InitializeAsync(appLifetime.ApplicationStopping);
                        _logger.LogInformation("Processor initialization completed successfully");
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Processor initialization cancelled during shutdown");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Processor initialization failed unexpectedly");
                    }
                });

                _logger.LogInformation(
                    "{ApplicationName} started successfully. Processor initialization is running in background with endless retry.",
                    GetType().Name);
            }
            else
            {
                // Legacy behavior: wait for initialization to complete
                await processorService.InitializeAsync();
                _logger.LogInformation("Processor initialization completed successfully");

                _logger.LogInformation(
                    "{ApplicationName} started successfully and is ready to process activities",
                    GetType().Name);
            }

            // Wait for shutdown signal
            var lifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
            await WaitForShutdownAsync(lifetime.ApplicationStopping);

            _logger.LogInformation("Shutting down {ApplicationName}", GetType().Name);

            // Stop the host gracefully
            await _host.StopAsync(TimeSpan.FromSeconds(30));

            _logger.LogInformation("{ApplicationName} stopped successfully", GetType().Name);

            Console.WriteLine($"✅ {processorName} Application completed successfully");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"🛑 {processorName} Application was cancelled");
            return 0;
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogCritical(ex, "Fatal error occurred in {ApplicationName}", GetType().Name);
            }

            Console.WriteLine($"💥 {processorName} Application terminated unexpectedly: {ex.Message}");
            Console.WriteLine($"🔍 Error Context:");
            Console.WriteLine($"   • Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"   • Message: {ex.Message}");
            Console.WriteLine($"   • Source: {ex.Source}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"   • Inner Exception: {ex.InnerException.Message}");
            }

            return 1;
        }
        finally
        {
            Console.WriteLine($"🧹 Shutting down {processorName} Application");
            _host?.Dispose();
        }
    }

    /// <summary>
    /// Initializes console output and displays startup information
    /// </summary>
    /// <returns>Tuple containing processor name and version</returns>
    private async Task<(string processorName, string processorVersion)> InitializeConsoleAndDisplayStartupInfoAsync()
    {
        // Force console output to be visible - bypass any logging framework interference
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.WriteLine("=== PROCESSOR STARTING ===");
        Console.WriteLine($"Current Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        // Test basic console output
        for (int i = 1; i <= 3; i++)
        {
            Console.WriteLine($"Console test {i}/3");
            System.Threading.Thread.Sleep(500);
        }

        // Load early configuration to get processor info
        var configuration = LoadEarlyConfiguration();
        var processorName = configuration["ProcessorConfiguration:Name"] ?? "Unknown";
        var processorVersion = configuration["ProcessorConfiguration:Version"] ?? "Unknown";

        // Display application information
        DisplayApplicationInformation(processorName, processorVersion);

        // Perform environment validation
        await ValidateEnvironmentAsync();

        // Display configuration
        await DisplayConfigurationAsync();

        return (processorName, processorVersion);
    }

    /// <summary>
    /// Loads early configuration before host is built
    /// </summary>
    /// <returns>Configuration instance</returns>
    private IConfiguration LoadEarlyConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Displays application information
    /// </summary>
    /// <param name="processorName">Name of the processor</param>
    /// <param name="processorVersion">Version of the processor</param>
    private void DisplayApplicationInformation(string processorName, string processorVersion)
    {
        Console.WriteLine($"🌟 Starting {processorName} v{processorVersion}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("📋 Application Information:");
        Console.WriteLine($"   🔖 Config Version: {processorVersion}");

        // Try to get implementation hash if available
        try
        {
            var hashType = Type.GetType("ProcessorImplementationHash");
            if (hashType != null)
            {
                var versionProp = hashType.GetProperty("Version");
                var hashProp = hashType.GetProperty("Hash");
                var sourceFileProp = hashType.GetProperty("SourceFile");
                var generatedAtProp = hashType.GetProperty("GeneratedAt");

                Console.WriteLine($"   📦 Assembly Version: {versionProp?.GetValue(null) ?? "Unknown"}");
                Console.WriteLine($"   🔐 SHA Hash: {hashProp?.GetValue(null) ?? "Unknown"}");
                Console.WriteLine($"   📝 Source File: {sourceFileProp?.GetValue(null) ?? "Unknown"}");
                Console.WriteLine($"   🕒 Hash Generated: {generatedAtProp?.GetValue(null) ?? "Unknown"}");
            }
        }
        catch
        {
            Console.WriteLine($"   📦 Assembly Version: Unknown");
            Console.WriteLine($"   🔐 SHA Hash: Unknown");
        }

        Console.WriteLine($"   🏷️  Processor Name: {processorName}");
        Console.WriteLine($"   🌍 Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
        Console.WriteLine($"   🖥️  Machine: {Environment.MachineName}");
        Console.WriteLine($"   👤 User: {Environment.UserName}");
        Console.WriteLine($"   📁 Working Directory: {Environment.CurrentDirectory}");
        Console.WriteLine($"   🆔 Process ID: {Environment.ProcessId}");
        Console.WriteLine($"   ⚙️  .NET Version: {Environment.Version}");
        Console.WriteLine($"   🕒 Started At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        // Allow derived classes to add custom application info
        DisplayCustomApplicationInfo();
    }

    /// <summary>
    /// Virtual method that derived classes can override to display custom application information
    /// </summary>
    protected virtual void DisplayCustomApplicationInfo()
    {
        // Default implementation does nothing
        // Derived classes can override to add custom information
    }

    /// <summary>
    /// Performs environment validation
    /// </summary>
    protected virtual async Task ValidateEnvironmentAsync()
    {
        Console.WriteLine("🔍 Performing Environment Validation...");

        var validationResults = new List<(string Check, bool Passed, string Message)>();

        // Check required environment variables
        var requiredEnvVars = new[] { "ASPNETCORE_ENVIRONMENT" };
        foreach (var envVar in requiredEnvVars)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            var passed = !string.IsNullOrEmpty(value);
            validationResults.Add((envVar, passed, passed ? $"✅ {envVar}={value}" : $"⚠️  {envVar} not set"));
        }

        // Check system resources
        var memoryMB = Environment.WorkingSet / 1024.0 / 1024.0;
        var memoryOk = memoryMB < 1000; // Less than 1GB
        validationResults.Add(("Memory", memoryOk,
            memoryOk ? $"✅ Memory usage: {memoryMB:F1} MB" : $"⚠️  High memory usage: {memoryMB:F1} MB"));

        // Check processor manager connectivity
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("http://localhost:5110/health");
            var passed = response.IsSuccessStatusCode;
            validationResults.Add(("ProcessorManager", passed,
                passed ? "✅ Processor Manager connectivity verified" : $"⚠️  Processor Manager returned: {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            validationResults.Add(("ProcessorManager", false, $"⚠️  Processor Manager unreachable: {ex.Message}"));
        }

        // Check schema manager connectivity
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("http://localhost:5100/health");
            var passed = response.IsSuccessStatusCode;
            validationResults.Add(("SchemaManager", passed,
                passed ? "✅ Schema Manager connectivity verified" : $"⚠️  Schema Manager returned: {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            validationResults.Add(("SchemaManager", false, $"⚠️  Schema Manager unreachable: {ex.Message}"));
        }

        // Allow derived classes to add custom validations
        await PerformCustomEnvironmentValidationAsync(validationResults);

        // Log validation results
        Console.WriteLine("📊 Environment Validation Results:");
        foreach (var (check, passed, message) in validationResults)
        {
            Console.WriteLine($"   {message}");
        }

        var passedCount = validationResults.Count(r => r.Passed);
        var totalCount = validationResults.Count;

        if (passedCount == totalCount)
        {
            Console.WriteLine($"🎉 All environment validations passed ({passedCount}/{totalCount})");
        }
        else
        {
            Console.WriteLine($"⚠️  Environment validation completed with warnings ({passedCount}/{totalCount} passed)");
            Console.WriteLine("💡 Warnings are normal if dependent services are not running yet");
        }

        Console.WriteLine("✅ Environment validation completed");
    }

    /// <summary>
    /// Virtual method that derived classes can override to add custom environment validations
    /// </summary>
    /// <param name="validationResults">List to add validation results to</param>
    protected virtual async Task PerformCustomEnvironmentValidationAsync(List<(string Check, bool Passed, string Message)> validationResults)
    {
        // Default implementation does nothing
        // Derived classes can override to add custom validations
        await Task.CompletedTask;
    }

    /// <summary>
    /// Displays configuration information
    /// </summary>
    protected virtual async Task DisplayConfigurationAsync()
    {
        Console.WriteLine("📋 Loading and Displaying Configuration...");

        try
        {
            var configuration = LoadEarlyConfiguration();

            // Read and display the entire appsettings.json content
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                var appSettingsContent = await File.ReadAllTextAsync(appSettingsPath);
                var formattedJson = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<object>(appSettingsContent),
                    new JsonSerializerOptions { WriteIndented = true });

                Console.WriteLine("📄 Configuration Content:");
                Console.WriteLine(formattedJson);
            }

            Console.WriteLine("✅ Configuration display completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error displaying configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures the host builder with all necessary services
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured host builder</returns>
    protected virtual IHostBuilder CreateHostBuilder(string[] args)
    {
        // Find the project directory by looking for the .csproj file
        var currentDir = Directory.GetCurrentDirectory();
        var projectDir = FindProjectDirectory(currentDir);

        return Host.CreateDefaultBuilder(args)
            .UseContentRoot(projectDir)
            .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .ConfigureLogging(logging =>
            {
                // Clear default logging providers - OpenTelemetry will handle logging
                logging.ClearProviders();
            })
            .ConfigureServices((context, services) =>
            {
                // Configure application settings
                services.Configure<ProcessorConfiguration>(
                    context.Configuration.GetSection("ProcessorConfiguration"));
                services.Configure<ProcessorHazelcastConfiguration>(
                    context.Configuration.GetSection("Hazelcast"));
                services.Configure<SchemaValidationConfiguration>(
                    context.Configuration.GetSection("SchemaValidation"));
                services.Configure<ProcessorInitializationConfiguration>(
                    context.Configuration.GetSection("ProcessorInitialization"));
                services.Configure<ProcessorHealthMonitorConfiguration>(
                    context.Configuration.GetSection("ProcessorHealthMonitor"));

                // Add core services
                services.AddSingleton<IActivityExecutor>(this);
                services.AddSingleton<IProcessorService, ProcessorService>();
                services.AddSingleton<ISchemaValidator, SchemaValidator>();

                // Add health monitoring services
                services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();
                services.AddSingleton<IProcessorHealthMonitor, ProcessorHealthMonitor>();
                services.AddHostedService<ProcessorHealthMonitor>();

                // Add infrastructure services
                services.AddMassTransitWithRabbitMq(context.Configuration);
                services.AddHazelcastClient(context.Configuration);
                services.AddOpenTelemetryObservability(context.Configuration);

                // Allow derived classes to add custom services
                ConfigureServices(services, context.Configuration);
            });
    }

    /// <summary>
    /// Virtual method that derived classes can override to add custom services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Default implementation does nothing
        // Derived classes can override to add custom services
    }

    /// <summary>
    /// Finds the project directory by looking for the .csproj file
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from</param>
    /// <returns>Project directory path</returns>
    private static string FindProjectDirectory(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);

        // Look for .csproj file in current directory and parent directories
        while (currentDir != null)
        {
            var csprojFiles = currentDir.GetFiles("*.csproj");
            if (csprojFiles.Length > 0)
            {
                return currentDir.FullName;
            }

            // Check if we're in the BaseProcessor.Application directory specifically
            if (currentDir.Name == "FlowOrchestrator.BaseProcessor.Application")
            {
                return currentDir.FullName;
            }

            currentDir = currentDir.Parent;
        }

        // Fallback: try to find the BaseProcessor.Application directory
        var baseDir = startDirectory;
        var targetPath = Path.Combine(baseDir, "src", "Framework", "FlowOrchestrator.BaseProcessor.Application");
        if (Directory.Exists(targetPath))
        {
            return targetPath;
        }

        // Final fallback: use current directory
        return startDirectory;
    }

    /// <summary>
    /// Waits for shutdown signal
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the wait operation</returns>
    private static async Task WaitForShutdownAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        cancellationToken.Register(() => tcs.SetResult(true));

        // Also listen for Ctrl+C
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            tcs.SetResult(true);
        };

        await tcs.Task;
    }
}


