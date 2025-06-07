using Shared.Entities.Base;
using Shared.Entities;
using Shared.Processor.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Processor.File;

/// <summary>
/// Sample concrete implementation of BaseProcessorApplication
/// Demonstrates how to create a specific processor service
/// The base class now provides a complete default implementation that can be overridden if needed
/// </summary>
public class FileProcessorApplication : BaseProcessorApplication
{


    /// <summary>
    /// Override to add console logging for debugging
    /// </summary>
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add console logging for debugging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Call base implementation
        base.ConfigureServices(services, configuration);
    }

    /// <summary>
    /// Concrete implementation of the activity processing logic
    /// This is where the specific processor business logic is implemented
    /// </summary>
    protected override async Task<ProcessedActivityData> ProcessActivityDataAsync(
        Guid processorId,
        Guid orchestratedFlowEntityId,
        Guid stepId,
        Guid executionId,
        List<BaseEntity> entities,
        JsonElement inputData,
        JsonElement? inputMetadata,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        // HASH VALIDATION TEST: This comment will change the implementation hash to demonstrate validation failure
        // Get logger from service provider
        var logger = ServiceProvider.GetRequiredService<ILogger<FileProcessorApplication>>();

        // Simulate some processing time
        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

        // Process the data (simplified from original implementation)
        logger.LogInformation(
            "Sample processor processing data. ProcessorId: {ProcessorId}, StepId: {StepId}",
            processorId, stepId);

        // Initialize sample data - will be assigned from entity data if available
        var sampleData = "This is sample data from the test processor";

        // Process AddressEntity if present and get its ConnectionString
        var addressEntities = entities.OfType<AddressEntity>().ToList();
        foreach (var address in addressEntities)
        {
            logger.LogInformation(
                "Processing AddressEntity '{AddressName}' with connection string: {ConnectionString}",
                address.Name, address.ConnectionString);

            // Use address data for sample processing
            sampleData = $"Processed address connection: {address.ConnectionString} for {address.Name}";
        }

        // Process DeliveryEntity if present and get its Payload
        var deliveryEntities = entities.OfType<DeliveryEntity>().ToList();
        foreach (var delivery in deliveryEntities)
        {
            logger.LogInformation(
                "Processing DeliveryEntity '{DeliveryName}' with payload",
                delivery.Name);

            // Use delivery payload for sample processing
            sampleData = $"Processed delivery payload: {delivery.Payload}";
        }

        // Log processing summary
        logger.LogInformation(
            "Completed processing entities. Total: {TotalEntities}, AddressEntities: {AddressCount}, DeliveryEntities: {DeliveryCount}, SampleData: {SampleData}",
            entities.Count, addressEntities.Count, deliveryEntities.Count, sampleData);

        // Return processed data
        return new ProcessedActivityData
        {
            Result = "Sample processing completed successfully",
            Status = "completed",
            Data = new
            {
                processorId = processorId.ToString(),
                orchestratedFlowEntityId = orchestratedFlowEntityId.ToString(),
                entitiesProcessed = entities.Count,
                addressEntitiesProcessed = addressEntities.Count,
                deliveryEntitiesProcessed = deliveryEntities.Count,
                processingDetails = new
                {
                    processedAt = DateTime.UtcNow,
                    processingDuration = "100ms",
                    inputDataReceived = true,
                    inputMetadataReceived = inputMetadata.HasValue,
                    sampleData = sampleData,
                    entityTypes = entities.Select(e => e.GetType().Name).Distinct().ToArray(),
                    addressEntities = addressEntities.Select(a => new
                    {
                        name = a.Name,
                        version = a.Version,
                        connectionString = a.ConnectionString,
                        schemaId = a.SchemaId.ToString(),
                        configuration = a.Configuration
                    }).ToArray(),
                    deliveryEntities = deliveryEntities.Select(d => new
                    {
                        name = d.Name,
                        version = d.Version,
                        schemaId = d.SchemaId.ToString(),
                        payload = d.Payload,
                        payloadUsedAsSampleData = d.Payload == sampleData
                    }).ToArray()
                }
            },
            ProcessorName = "FileProcessor",
            Version = "1.0",
            ExecutionId = executionId == Guid.Empty ? Guid.NewGuid() : executionId
        };
    }
}