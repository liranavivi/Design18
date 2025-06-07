using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Processor.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.MassTransit.Commands;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Processor;

public class GetProcessorQueryConsumer : IConsumer<GetProcessorQuery>
{
    private readonly IProcessorEntityRepository _repository;
    private readonly ILogger<GetProcessorQueryConsumer> _logger;

    public GetProcessorQueryConsumer(
        IProcessorEntityRepository repository,
        ILogger<GetProcessorQueryConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetProcessorQuery> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = context.Message;

        _logger.LogInformation("Processing GetProcessorQuery. Id: {Id}, CompositeKey: {CompositeKey}",
            query.Id, query.CompositeKey);

        try
        {
            ProcessorEntity? entity = null;

            if (query.Id.HasValue)
            {
                entity = await _repository.GetByIdAsync(query.Id.Value);
            }
            else if (!string.IsNullOrEmpty(query.CompositeKey))
            {
                entity = await _repository.GetByCompositeKeyAsync(query.CompositeKey);
            }

            stopwatch.Stop();

            if (entity != null)
            {
                _logger.LogInformation("Successfully processed GetProcessorQuery. Found entity Id: {Id}, Duration: {Duration}ms",
                    entity.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetProcessorQueryResponse
                {
                    Success = true,
                    Entity = entity,
                    Message = "Processor entity found"
                });
            }
            else
            {
                _logger.LogInformation("Processor entity not found. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                    query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetProcessorQueryResponse
                {
                    Success = false,
                    Entity = null,
                    Message = "Processor entity not found"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing GetProcessorQuery. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new GetProcessorQueryResponse
            {
                Success = false,
                Entity = null,
                Message = $"Error retrieving Processor entity: {ex.Message}"
            });
        }
    }
}
