using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Extensions;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Interfaces;
using FlowOrchestrator.Infrastructure.Scheduling.Quartz.Services;
using FlowOrchestrator.Management.Scheduling;
using FlowOrchestrator.Management.Scheduling.Configuration;
using FlowOrchestrator.Management.Scheduling.Messaging.Commands;
using FlowOrchestrator.Management.Scheduling.Messaging.Consumers;
using FlowOrchestrator.Management.Scheduling.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure services
builder.Services.Configure<TaskSchedulerOptions>(builder.Configuration.GetSection("TaskScheduler"));

// Add Quartz scheduling
builder.Services.AddQuartzScheduling(options =>
{
    options.SchedulerName = "FlowOrchestratorTaskScheduler";
    options.InstanceId = "AUTO";
    options.ThreadCount = 10;
    options.MakeSchedulerThreadDaemon = true;
    options.AutoStart = true;
    options.WaitForJobsToCompleteOnShutdown = true;
});

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure message consumers
    x.AddConsumer<TriggerFlowCommandConsumer>();
    x.AddConsumer<ScheduleFlowCommandConsumer>();

    // Configure the bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Register message bus
builder.Services.AddSingleton<IMessageBus, MassTransitMessageBus>();

// Register services
builder.Services.AddSingleton<TaskSchedulerService>();
builder.Services.AddSingleton<FlowExecutionService>();
builder.Services.AddSingleton<SchedulerManager>();

// Register worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
