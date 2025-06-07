using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Address.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.MassTransit.Commands;
using Shared.MassTransit.Events;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Address;

public class CreateAddressCommandConsumer : IConsumer<CreateAddressCommand>
{
    private readonly IAddressEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateAddressCommandConsumer> _logger;

    public CreateAddressCommandConsumer(
        IAddressEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateAddressCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateAddressCommand> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var command = context.Message;

        _logger.LogInformation("Processing CreateAddressCommand. Version: {Version}, Name: {Name}, ConnectionString: {ConnectionString}, RequestedBy: {RequestedBy}",
            command.Version, command.Name, command.ConnectionString, command.RequestedBy);

        try
        {
            var entity = new AddressEntity
            {
                Version = command.Version,
                Name = command.Name,
                Description = command.Description,
                ConnectionString = command.ConnectionString,
                Configuration = command.Configuration,
                SchemaId = command.SchemaId,
                CreatedBy = command.RequestedBy
            };

            var created = await _repository.CreateAsync(entity);

            await _publishEndpoint.Publish(new AddressCreatedEvent
            {
                Id = created.Id,
                Version = created.Version,
                Name = created.Name,
                Description = created.Description,
                ConnectionString = created.ConnectionString,
                Configuration = created.Configuration,
                SchemaId = created.SchemaId,
                CreatedAt = created.CreatedAt,
                CreatedBy = created.CreatedBy
            });

            stopwatch.Stop();
            _logger.LogInformation("Successfully processed CreateAddressCommand. Id: {Id}, Duration: {Duration}ms",
                created.Id, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new CreateAddressCommandResponse
            {
                Success = true,
                Id = created.Id,
                Message = "Address entity created successfully"
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing CreateAddressCommand. Version: {Version}, Name: {Name}, ConnectionString: {ConnectionString}, Duration: {Duration}ms",
                command.Version, command.Name, command.ConnectionString, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new CreateAddressCommandResponse
            {
                Success = false,
                Id = Guid.Empty,
                Message = $"Failed to create Address entity: {ex.Message}"
            });
        }
    }
}

public class CreateAddressCommandResponse
{
    public bool Success { get; set; }
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
