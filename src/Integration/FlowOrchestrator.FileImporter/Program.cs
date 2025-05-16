using FlowOrchestrator.FileImporter;
using FlowOrchestrator.Infrastructure.Telemetry.OpenTelemetry.Extensions;
using FlowOrchestrator.Integration.Importers.File;
using FlowOrchestrator.Integration.Importers.File.Utilities;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<FileImporterService>();
builder.Services.AddSingleton<FileImporterServiceWithTelemetry>();
builder.Services.AddHostedService<Worker>();

// Add OpenTelemetry
builder.Services.AddFlowOrchestratorOpenTelemetry(builder.Configuration);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddFlowOrchestratorOpenTelemetryLogging(builder.Configuration);
});

var host = builder.Build();
host.Run();
