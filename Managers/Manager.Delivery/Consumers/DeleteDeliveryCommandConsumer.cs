using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Delivery.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.MassTransit.Commands;
using Shared.MassTransit.Events;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Delivery;

public class DeleteDeliveryCommandConsumer : IConsumer<DeleteDeliveryCommand>
{
    private readonly IDeliveryEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DeleteDeliveryCommandConsumer> _logger;

    public DeleteDeliveryCommandConsumer(
        IDeliveryEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<DeleteDeliveryCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteDeliveryCommand> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var command = context.Message;

        _logger.LogInformation("Processing DeleteDeliveryCommand. Id: {Id}, RequestedBy: {RequestedBy}",
            command.Id, command.RequestedBy);

        try
        {
            var existingEntity = await _repository.GetByIdAsync(command.Id);
            if (existingEntity == null)
            {
                _logger.LogWarning("Delivery entity not found for deletion. Id: {Id}", command.Id);
                await context.RespondAsync(new DeleteDeliveryCommandResponse
                {
                    Success = false,
                    Message = $"Delivery entity with ID {command.Id} not found"
                });
                return;
            }

            var success = await _repository.DeleteAsync(command.Id);

            if (success)
            {
                await _publishEndpoint.Publish(new DeliveryDeletedEvent
                {
                    Id = command.Id,
                    DeletedAt = DateTime.UtcNow,
                    DeletedBy = command.RequestedBy
                });

                stopwatch.Stop();
                _logger.LogInformation("Successfully processed DeleteDeliveryCommand. Id: {Id}, Duration: {Duration}ms",
                    command.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new DeleteDeliveryCommandResponse
                {
                    Success = true,
                    Message = "Delivery entity deleted successfully"
                });
            }
            else
            {
                stopwatch.Stop();
                _logger.LogWarning("Failed to delete Delivery entity. Id: {Id}, Duration: {Duration}ms",
                    command.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new DeleteDeliveryCommandResponse
                {
                    Success = false,
                    Message = "Failed to delete Delivery entity"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing DeleteDeliveryCommand. Id: {Id}, Duration: {Duration}ms",
                command.Id, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new DeleteDeliveryCommandResponse
            {
                Success = false,
                Message = $"Failed to delete Delivery entity: {ex.Message}"
            });
        }
    }
}

public class DeleteDeliveryCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
