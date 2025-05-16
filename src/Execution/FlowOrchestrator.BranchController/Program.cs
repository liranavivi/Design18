using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.BranchController;
using FlowOrchestrator.BranchController.Messaging.Commands;
using FlowOrchestrator.BranchController.Messaging.Consumers;
using FlowOrchestrator.BranchController.Services;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddSingleton<BranchContextService>();
builder.Services.AddSingleton<BranchIsolationService>();
builder.Services.AddSingleton<BranchCompletionService>();

// Register message consumers
builder.Services.AddSingleton<CreateBranchCommandConsumer>();
builder.Services.AddSingleton<UpdateBranchStatusCommandConsumer>();
builder.Services.AddSingleton<AddCompletedStepCommandConsumer>();
builder.Services.AddSingleton<AddPendingStepCommandConsumer>();

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure the bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Register message bus
builder.Services.AddSingleton<IMessageBus, FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation.MassTransitMessageBus>();

// Register message consumer adapters
builder.Services.AddSingleton<MassTransit.IConsumer<CreateBranchCommand>>(provider =>
    new FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<CreateBranchCommand>(
        provider.GetRequiredService<CreateBranchCommandConsumer>(),
        provider.GetRequiredService<ILogger<FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<CreateBranchCommand>>>()));

builder.Services.AddSingleton<MassTransit.IConsumer<UpdateBranchStatusCommand>>(provider =>
    new FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<UpdateBranchStatusCommand>(
        provider.GetRequiredService<UpdateBranchStatusCommandConsumer>(),
        provider.GetRequiredService<ILogger<FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<UpdateBranchStatusCommand>>>()));

builder.Services.AddSingleton<MassTransit.IConsumer<AddCompletedStepCommand>>(provider =>
    new FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<AddCompletedStepCommand>(
        provider.GetRequiredService<AddCompletedStepCommandConsumer>(),
        provider.GetRequiredService<ILogger<FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<AddCompletedStepCommand>>>()));

builder.Services.AddSingleton<MassTransit.IConsumer<AddPendingStepCommand>>(provider =>
    new FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<AddPendingStepCommand>(
        provider.GetRequiredService<AddPendingStepCommandConsumer>(),
        provider.GetRequiredService<ILogger<FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters.ConsumerAdapter<AddPendingStepCommand>>>()));

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("BranchController", () => HealthCheckResult.Healthy("Branch Controller is running"));

// Add worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
