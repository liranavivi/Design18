using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Address.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.Exceptions;
using Shared.MassTransit.Commands;
using Shared.MassTransit.Events;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Address;

public class UpdateAddressCommandConsumer : IConsumer<UpdateAddressCommand>
{
    private readonly IAddressEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateAddressCommandConsumer> _logger;

    public UpdateAddressCommandConsumer(
        IAddressEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<UpdateAddressCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateAddressCommand> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var command = context.Message;

        _logger.LogInformation("Processing UpdateAddressCommand. Id: {Id}, Version: {Version}, Name: {Name}, ConnectionString: {ConnectionString}, RequestedBy: {RequestedBy}",
            command.Id, command.Version, command.Name, command.ConnectionString, command.RequestedBy);

        try
        {
            var existingEntity = await _repository.GetByIdAsync(command.Id);
            if (existingEntity == null)
            {
                _logger.LogWarning("Address entity not found for update. Id: {Id}", command.Id);
                await context.RespondAsync(new UpdateAddressCommandResponse
                {
                    Success = false,
                    Message = $"Address entity with ID {command.Id} not found"
                });
                return;
            }

            var entity = new AddressEntity
            {
                Id = command.Id,
                Version = command.Version,
                Name = command.Name,
                Description = command.Description,
                ConnectionString = command.ConnectionString,
                Configuration = command.Configuration,
                SchemaId = command.SchemaId,
                UpdatedBy = command.RequestedBy,
                CreatedAt = existingEntity.CreatedAt,
                CreatedBy = existingEntity.CreatedBy
            };

            var updated = await _repository.UpdateAsync(entity);

            await _publishEndpoint.Publish(new AddressUpdatedEvent
            {
                Id = updated.Id,
                Version = updated.Version,
                Name = updated.Name,
                Description = updated.Description,
                ConnectionString = updated.ConnectionString,
                Configuration = updated.Configuration,
                SchemaId = updated.SchemaId,
                UpdatedAt = updated.UpdatedAt,
                UpdatedBy = updated.UpdatedBy
            });

            stopwatch.Stop();
            _logger.LogInformation("Successfully processed UpdateAddressCommand. Id: {Id}, Duration: {Duration}ms",
                updated.Id, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new UpdateAddressCommandResponse
            {
                Success = true,
                Message = "Address entity updated successfully"
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing UpdateAddressCommand. Id: {Id}, Duration: {Duration}ms",
                command.Id, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new UpdateAddressCommandResponse
            {
                Success = false,
                Message = $"Failed to update Address entity: {ex.Message}"
            });
        }
    }
}

public class UpdateAddressCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
