using FlowOrchestrator.Integration.Exporters.File;
using FlowOrchestrator.Integration.Exporters.File.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configure services
builder.Services.Configure<FileExporterConfiguration>(
    builder.Configuration.GetSection("FileExporter"));

// Register the file exporter service
builder.Services.AddSingleton<FileExporterService>();

// Register the worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
