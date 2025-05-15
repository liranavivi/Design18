using FlowOrchestrator.FileImporter;
using FlowOrchestrator.Integration.Importers.File;
using FlowOrchestrator.Integration.Importers.File.Utilities;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<FileImporterService>();
builder.Services.AddHostedService<Worker>();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var host = builder.Build();
host.Run();
