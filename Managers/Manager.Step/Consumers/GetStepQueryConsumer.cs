using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Step.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.MassTransit.Commands;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Step;

public class GetStepQueryConsumer : IConsumer<GetStepQuery>
{
    private readonly IStepEntityRepository _repository;
    private readonly ILogger<GetStepQueryConsumer> _logger;

    public GetStepQueryConsumer(
        IStepEntityRepository repository,
        ILogger<GetStepQueryConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetStepQuery> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = context.Message;

        _logger.LogInformation("Processing GetStepQuery. Id: {Id}, CompositeKey: {CompositeKey}",
            query.Id, query.CompositeKey);

        try
        {
            StepEntity? entity = null;

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
                _logger.LogInformation("Successfully processed GetStepQuery. Found entity Id: {Id}, Duration: {Duration}ms",
                    entity.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetStepQueryResponse
                {
                    Success = true,
                    Entity = entity,
                    Message = "Step entity found"
                });
            }
            else
            {
                _logger.LogInformation("Step entity not found. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                    query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetStepQueryResponse
                {
                    Success = false,
                    Entity = null,
                    Message = "Step entity not found"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing GetStepQuery. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new GetStepQueryResponse
            {
                Success = false,
                Entity = null,
                Message = $"Error retrieving Step entity: {ex.Message}"
            });
        }
    }
}

public class GetStepQueryResponse
{
    public bool Success { get; set; }
    public StepEntity? Entity { get; set; }
    public string Message { get; set; } = string.Empty;
}
