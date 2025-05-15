using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Defines the interface for processor services in the FlowOrchestrator system.
    /// </summary>
    public interface IProcessorService : IService
    {
        /// <summary>
        /// Processes data.
        /// </summary>
        /// <param name="parameters">The process parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The processing result.</returns>
        ProcessingResult Process(ProcessParameters parameters, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets the capabilities of the processor service.
        /// </summary>
        /// <returns>The processor capabilities.</returns>
        ProcessorCapabilities GetCapabilities();

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema definition.</returns>
        SchemaDefinition GetInputSchema();

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema definition.</returns>
        SchemaDefinition GetOutputSchema();

        /// <summary>
        /// Validates the process parameters.
        /// </summary>
        /// <param name="parameters">The process parameters to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateParameters(ProcessParameters parameters);
    }

    /// <summary>
    /// Represents the parameters for a process operation.
    /// </summary>
    public class ProcessParameters
    {
        /// <summary>
        /// Gets or sets the input data to process.
        /// </summary>
        public DataPackage InputData { get; set; } = new DataPackage();

        /// <summary>
        /// Gets or sets the schema identifier for validation.
        /// </summary>
        public string? OutputSchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema version for validation.
        /// </summary>
        public string? OutputSchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the processing options.
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the result of a processing operation.
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the processing was successful.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the transformed data.
        /// </summary>
        public DataPackage? TransformedData { get; set; }

        /// <summary>
        /// Gets or sets the transformation metadata.
        /// </summary>
        public Dictionary<string, object> TransformationMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the validation results.
        /// </summary>
        public ValidationResult? ValidationResults { get; set; }

        /// <summary>
        /// Gets or sets the statistics for the processing operation.
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the error information if the processing failed.
        /// </summary>
        public ExecutionError? Error { get; set; }
    }

    /// <summary>
    /// Represents the capabilities of a processor service.
    /// </summary>
    public class ProcessorCapabilities
    {
        /// <summary>
        /// Gets or sets the supported input data formats.
        /// </summary>
        public List<string> SupportedInputFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported output data formats.
        /// </summary>
        public List<string> SupportedOutputFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported transformation types.
        /// </summary>
        public List<string> SupportedTransformationTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported schema formats.
        /// </summary>
        public List<SchemaFormat> SupportedSchemaFormats { get; set; } = new List<SchemaFormat>();

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports validation.
        /// </summary>
        public bool SupportsValidation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports streaming.
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports batching.
        /// </summary>
        public bool SupportsBatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports filtering.
        /// </summary>
        public bool SupportsFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports aggregation.
        /// </summary>
        public bool SupportsAggregation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the processor supports enrichment.
        /// </summary>
        public bool SupportsEnrichment { get; set; }

        /// <summary>
        /// Gets or sets the maximum batch size supported by the processor.
        /// </summary>
        public int? MaxBatchSize { get; set; }
    }
}
