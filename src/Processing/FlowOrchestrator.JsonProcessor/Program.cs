using FlowOrchestrator.Processing.Json;
using FlowOrchestrator.Processing.Json.Configuration;
using FlowOrchestrator.Processing.Json.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configure services
builder.Services.Configure<JsonProcessorConfiguration>(
    builder.Configuration.GetSection("JsonProcessor"));

// Register the JSON processor service
builder.Services.AddSingleton<JsonProcessorService>();

// Register the worker service
builder.Services.AddHostedService<Worker>();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var host = builder.Build();
host.Run();
