using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Recovery;
using FlowOrchestrator.Recovery.Messaging;
using FlowOrchestrator.Recovery.Services;
using FlowOrchestrator.Recovery.Strategies;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddSingleton<RecoveryService>();
builder.Services.AddSingleton<ErrorClassificationService>();

// Add recovery strategies
builder.Services.AddSingleton<IRecoveryStrategy, RetryRecoveryStrategy>();
builder.Services.AddSingleton<IRecoveryStrategy, CircuitBreakerRecoveryStrategy>();
builder.Services.AddSingleton<IRecoveryStrategy, FallbackRecoveryStrategy>();
builder.Services.AddSingleton<IRecoveryStrategy, BulkheadRecoveryStrategy>();
builder.Services.AddSingleton<IRecoveryStrategy, CompensationRecoveryStrategy>();
builder.Services.AddSingleton<IRecoveryStrategy, TimeoutRecoveryStrategy>();

// Add message consumers
builder.Services.AddSingleton<ErrorEventConsumer>();
builder.Services.AddSingleton<RecoveryCommandConsumer>();

// Configure options
builder.Services.Configure<RecoveryServiceOptions>(builder.Configuration.GetSection("Recovery"));

// Register in-memory message bus
builder.Services.AddSingleton<IMessageBus, InMemoryMessageBus>();

// Add worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
