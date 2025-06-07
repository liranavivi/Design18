using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.OrchestratedFlow.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.MassTransit.Commands;
using Shared.MassTransit.Events;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.OrchestratedFlow;

public class DeleteOrchestratedFlowCommandConsumer : IConsumer<DeleteOrchestratedFlowCommand>
{
    private readonly IOrchestratedFlowEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DeleteOrchestratedFlowCommandConsumer> _logger;

    public DeleteOrchestratedFlowCommandConsumer(
        IOrchestratedFlowEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<DeleteOrchestratedFlowCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteOrchestratedFlowCommand> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var command = context.Message;

        _logger.LogInformation("Processing DeleteOrchestratedFlowCommand. Id: {Id}, RequestedBy: {RequestedBy}",
            command.Id, command.RequestedBy);

        try
        {
            var existingEntity = await _repository.GetByIdAsync(command.Id);
            if (existingEntity == null)
            {
                _logger.LogWarning("OrchestratedFlow entity not found for deletion. Id: {Id}", command.Id);
                await context.RespondAsync(new DeleteOrchestratedFlowCommandResponse
                {
                    Success = false,
                    Message = $"OrchestratedFlow entity with ID {command.Id} not found"
                });
                return;
            }

            var success = await _repository.DeleteAsync(command.Id);

            if (success)
            {
                await _publishEndpoint.Publish(new OrchestratedFlowDeletedEvent
                {
                    Id = command.Id,
                    DeletedAt = DateTime.UtcNow,
                    DeletedBy = command.RequestedBy
                });

                stopwatch.Stop();
                _logger.LogInformation("Successfully processed DeleteOrchestratedFlowCommand. Id: {Id}, Duration: {Duration}ms",
                    command.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new DeleteOrchestratedFlowCommandResponse
                {
                    Success = true,
                    Message = "OrchestratedFlow entity deleted successfully"
                });
            }
            else
            {
                stopwatch.Stop();
                _logger.LogWarning("Failed to delete OrchestratedFlow entity. Id: {Id}, Duration: {Duration}ms",
                    command.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new DeleteOrchestratedFlowCommandResponse
                {
                    Success = false,
                    Message = "Failed to delete OrchestratedFlow entity"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing DeleteOrchestratedFlowCommand. Id: {Id}, Duration: {Duration}ms",
                command.Id, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new DeleteOrchestratedFlowCommandResponse
            {
                Success = false,
                Message = $"Failed to delete OrchestratedFlow entity: {ex.Message}"
            });
        }
    }
}

public class DeleteOrchestratedFlowCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
