using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions;
using FlowOrchestrator.MemoryManager;
using FlowOrchestrator.MemoryManager.Cache;
using FlowOrchestrator.MemoryManager.Interfaces;
using FlowOrchestrator.MemoryManager.Messaging;
using FlowOrchestrator.MemoryManager.Messaging.Consumers;
using FlowOrchestrator.MemoryManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register memory manager services
builder.Services.AddSingleton<IMemoryManager, MemoryManagerService>();
builder.Services.AddSingleton<IMemoryAllocationService, MemoryAllocationService>();
builder.Services.AddSingleton<IMemoryAccessControlService, MemoryAccessControlService>();

// Register Hazelcast services
// Note: This is a placeholder. The actual implementation would use the Hazelcast extension methods.
builder.Services.AddSingleton<FlowOrchestrator.Infrastructure.Data.Hazelcast.Cache.IDistributedCache, HazelcastDistributedCache>();

// Configure MassTransit
builder.Services.AddFlowOrchestratorMessageBus(options =>
{
    builder.Configuration.GetSection("MessageBus").Bind(options);
});

// Register message consumers
builder.Services.AddFlowOrchestratorConsumer<MemoryAllocationCommand, MemoryAllocationCommandConsumer>();

// Register worker service
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
