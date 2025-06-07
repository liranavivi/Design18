using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Manager.Workflow.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.MassTransit.Commands;

namespace FlowOrchestrator.EntitiesManagers.Infrastructure.MassTransit.Consumers.Workflow;

public class GetWorkflowStepsQueryConsumer : IConsumer<GetWorkflowStepsQuery>
{
    private readonly IWorkflowEntityRepository _repository;
    private readonly ILogger<GetWorkflowStepsQueryConsumer> _logger;

    public GetWorkflowStepsQueryConsumer(
        IWorkflowEntityRepository repository,
        ILogger<GetWorkflowStepsQueryConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetWorkflowStepsQuery> context)
    {
        var query = context.Message;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Processing GetWorkflowStepsQuery. WorkflowId: {WorkflowId}, RequestedBy: {RequestedBy}",
            query.WorkflowId, query.RequestedBy);

        try
        {
            var entity = await _repository.GetByIdAsync(query.WorkflowId);

            stopwatch.Stop();

            if (entity != null)
            {
                _logger.LogInformation("Successfully processed GetWorkflowStepsQuery. Found Workflow Id: {Id}, StepIds count: {StepIdsCount}, Duration: {Duration}ms",
                    entity.Id, entity.StepIds?.Count ?? 0, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetWorkflowStepsQueryResponse
                {
                    Success = true,
                    StepIds = entity.StepIds,
                    Message = "Workflow step IDs retrieved successfully"
                });
            }
            else
            {
                _logger.LogWarning("Workflow entity not found. WorkflowId: {WorkflowId}, Duration: {Duration}ms",
                    query.WorkflowId, stopwatch.ElapsedMilliseconds);

                await context.RespondAsync(new GetWorkflowStepsQueryResponse
                {
                    Success = false,
                    StepIds = null,
                    Message = $"Workflow entity with ID {query.WorkflowId} not found"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing GetWorkflowStepsQuery. WorkflowId: {WorkflowId}, Duration: {Duration}ms",
                query.WorkflowId, stopwatch.ElapsedMilliseconds);

            await context.RespondAsync(new GetWorkflowStepsQueryResponse
            {
                Success = false,
                StepIds = null,
                Message = $"Error retrieving Workflow step IDs: {ex.Message}"
            });
        }
    }
}
