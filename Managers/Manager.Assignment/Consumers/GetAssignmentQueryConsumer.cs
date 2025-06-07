using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Assignment.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.MassTransit.Commands;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Assignment;

public class GetAssignmentQueryConsumer : IConsumer<GetAssignmentQuery>
{
    private readonly IAssignmentEntityRepository _repository;
    private readonly ILogger<GetAssignmentQueryConsumer> _logger;

    public GetAssignmentQueryConsumer(
        IAssignmentEntityRepository repository,
        ILogger<GetAssignmentQueryConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetAssignmentQuery> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = context.Message;

        _logger.LogInformation("Processing GetAssignmentQuery. Id: {Id}, CompositeKey: {CompositeKey}",
            query.Id, query.CompositeKey);

        try
        {
            AssignmentEntity? entity = null;

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
                _logger.LogInformation("Successfully processed GetAssignmentQuery. Found entity Id: {Id}, Duration: {Duration}ms",
                    entity.Id, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetAssignmentQueryResponse
                {
                    Success = true,
                    Entity = entity,
                    Message = "Assignment entity found"
                });
            }
            else
            {
                _logger.LogInformation("Assignment entity not found. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                    query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetAssignmentQueryResponse
                {
                    Success = false,
                    Entity = null,
                    Message = "Assignment entity not found"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing GetAssignmentQuery. Id: {Id}, CompositeKey: {CompositeKey}, Duration: {Duration}ms",
                query.Id, query.CompositeKey, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new GetAssignmentQueryResponse
            {
                Success = false,
                Entity = null,
                Message = $"Error retrieving Assignment entity: {ex.Message}"
            });
        }
    }
}

public class GetAssignmentQueryResponse
{
    public bool Success { get; set; }
    public AssignmentEntity? Entity { get; set; }
    public string Message { get; set; } = string.Empty;
}
