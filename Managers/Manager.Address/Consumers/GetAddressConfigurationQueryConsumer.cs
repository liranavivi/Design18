using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Address.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.MassTransit.Commands;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Address;

public class GetAddressConfigurationQueryConsumer : IConsumer<GetAddressConfigurationQuery>
{
    private readonly IAddressEntityRepository _repository;
    private readonly ILogger<GetAddressConfigurationQueryConsumer> _logger;

    public GetAddressConfigurationQueryConsumer(
        IAddressEntityRepository repository,
        ILogger<GetAddressConfigurationQueryConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetAddressConfigurationQuery> context)
    {
        var query = context.Message;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Processing GetAddressConfigurationQuery. AddressId: {AddressId}, RequestedBy: {RequestedBy}",
            query.AddressId, query.RequestedBy);

        try
        {
            var entity = await _repository.GetByIdAsync(query.AddressId);

            stopwatch.Stop();

            if (entity != null)
            {
                _logger.LogInformation("Successfully processed GetAddressConfigurationQuery. Found Address Id: {Id}, Configuration items: {ConfigurationCount}, Duration: {Duration}ms",
                    entity.Id, entity.Configuration?.Count ?? 0, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetAddressConfigurationQueryResponse
                {
                    Success = true,
                    Configuration = entity.Configuration,
                    Message = "Address configuration retrieved successfully"
                });
            }
            else
            {
                _logger.LogWarning("Address entity not found. AddressId: {AddressId}, Duration: {Duration}ms",
                    query.AddressId, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetAddressConfigurationQueryResponse
                {
                    Success = false,
                    Configuration = null,
                    Message = $"Address entity with ID {query.AddressId} not found"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing GetAddressConfigurationQuery. AddressId: {AddressId}, Duration: {Duration}ms",
                query.AddressId, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new GetAddressConfigurationQueryResponse
            {
                Success = false,
                Configuration = null,
                Message = $"Error retrieving Address configuration: {ex.Message}"
            });
        }
    }
}
