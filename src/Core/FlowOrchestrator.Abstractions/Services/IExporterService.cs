using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Defines the interface for exporter services in the FlowOrchestrator system.
    /// </summary>
    public interface IExporterService : IService
    {
        /// <summary>
        /// Gets the protocol used by this exporter service.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Exports data to a destination.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        ExportResult Export(ExportParameters parameters, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets the capabilities of the exporter service.
        /// </summary>
        /// <returns>The exporter capabilities.</returns>
        ExporterCapabilities GetCapabilities();

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateParameters(ExportParameters parameters);

        /// <summary>
        /// Merges data from multiple branches.
        /// </summary>
        /// <param name="branchData">The data from each branch.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The export result.</returns>
        ExportResult MergeBranches(Dictionary<string, DataPackage> branchData, MergeStrategy strategy, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets the merge capabilities of the exporter service.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        MergeCapabilities GetMergeCapabilities();
    }

    /// <summary>
    /// Represents the parameters for an export operation.
    /// </summary>
    public class ExportParameters
    {
        /// <summary>
        /// Gets or sets the data to export.
        /// </summary>
        public DataPackage Data { get; set; } = new DataPackage();

        /// <summary>
        /// Gets or sets the destination identifier.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination location.
        /// </summary>
        public string DestinationLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination configuration.
        /// </summary>
        public Dictionary<string, object> DestinationConfiguration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the export options.
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the result of an export operation.
    /// </summary>
    public class ExportResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the export was successful.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the destination metadata.
        /// </summary>
        public Dictionary<string, object> DestinationMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the destination information.
        /// </summary>
        public DestinationInformation? Destination { get; set; }

        /// <summary>
        /// Gets or sets the statistics for the export operation.
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the error information if the export failed.
        /// </summary>
        public ExecutionError? Error { get; set; }
    }

    /// <summary>
    /// Represents information about the destination of data.
    /// </summary>
    public class DestinationInformation
    {
        /// <summary>
        /// Gets or sets the destination identifier.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination type.
        /// </summary>
        public string DestinationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination location.
        /// </summary>
        public string DestinationLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination configuration.
        /// </summary>
        public ConfigurationParameters? Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationInformation"/> class.
        /// </summary>
        public DestinationInformation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationInformation"/> class with the specified configuration.
        /// </summary>
        /// <param name="configuration">The destination configuration.</param>
        public DestinationInformation(ConfigurationParameters configuration)
        {
            Configuration = configuration;

            if (configuration.TryGetParameter<string>("DestinationId", out var destinationId))
            {
                DestinationId = destinationId;
            }

            if (configuration.TryGetParameter<string>("DestinationType", out var destinationType))
            {
                DestinationType = destinationType;
            }

            if (configuration.TryGetParameter<string>("DestinationLocation", out var destinationLocation))
            {
                DestinationLocation = destinationLocation;
            }
        }
    }

    /// <summary>
    /// Represents the capabilities of an exporter service.
    /// </summary>
    public class ExporterCapabilities
    {
        /// <summary>
        /// Gets or sets the supported destination types.
        /// </summary>
        public List<string> SupportedDestinationTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported data formats.
        /// </summary>
        public List<string> SupportedDataFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported merge strategies.
        /// </summary>
        public List<MergeStrategy> SupportedMergeStrategies { get; set; } = new List<MergeStrategy>();

        /// <summary>
        /// Gets or sets a value indicating whether the exporter supports streaming.
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exporter supports batching.
        /// </summary>
        public bool SupportsBatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exporter supports transactions.
        /// </summary>
        public bool SupportsTransactions { get; set; }

        /// <summary>
        /// Gets or sets the maximum batch size supported by the exporter.
        /// </summary>
        public int? MaxBatchSize { get; set; }
    }

    /// <summary>
    /// Represents a strategy for merging data from multiple branches.
    /// </summary>
    public enum MergeStrategy
    {
        /// <summary>
        /// Concatenate the data from all branches.
        /// </summary>
        CONCATENATE,

        /// <summary>
        /// Merge the data from all branches.
        /// </summary>
        MERGE,

        /// <summary>
        /// Aggregate the data from all branches.
        /// </summary>
        AGGREGATE,

        /// <summary>
        /// Use the data from the first branch only.
        /// </summary>
        FIRST_ONLY,

        /// <summary>
        /// Use the data from the last branch only.
        /// </summary>
        LAST_ONLY,

        /// <summary>
        /// Use a custom merge strategy.
        /// </summary>
        CUSTOM
    }
}
