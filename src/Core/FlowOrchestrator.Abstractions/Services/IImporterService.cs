using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Services
{
    /// <summary>
    /// Defines the interface for importer services in the FlowOrchestrator system.
    /// </summary>
    public interface IImporterService : IService
    {
        /// <summary>
        /// Gets the protocol used by this importer service.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Imports data from a source.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>The import result.</returns>
        ImportResult Import(ImportParameters parameters, FlowOrchestrator.Abstractions.Common.ExecutionContext context);

        /// <summary>
        /// Gets the capabilities of the importer service.
        /// </summary>
        /// <returns>The importer capabilities.</returns>
        ImporterCapabilities GetCapabilities();

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>The validation result.</returns>
        ValidationResult ValidateParameters(ImportParameters parameters);
    }

    /// <summary>
    /// Represents the parameters for an import operation.
    /// </summary>
    public class ImportParameters
    {
        /// <summary>
        /// Gets or sets the source identifier.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source location.
        /// </summary>
        public string SourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source configuration.
        /// </summary>
        public Dictionary<string, object> SourceConfiguration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the schema identifier for validation.
        /// </summary>
        public string? SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema version for validation.
        /// </summary>
        public string? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the import options.
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the result of an import operation.
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the import was successful.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the imported data.
        /// </summary>
        public DataPackage? Data { get; set; }

        /// <summary>
        /// Gets or sets the source metadata.
        /// </summary>
        public Dictionary<string, object> SourceMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the source information.
        /// </summary>
        public SourceInformation? Source { get; set; }

        /// <summary>
        /// Gets or sets the validation results.
        /// </summary>
        public ValidationResult? ValidationResults { get; set; }

        /// <summary>
        /// Gets or sets the statistics for the import operation.
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the error information if the import failed.
        /// </summary>
        public ExecutionError? Error { get; set; }
    }

    /// <summary>
    /// Represents the capabilities of an importer service.
    /// </summary>
    public class ImporterCapabilities
    {
        /// <summary>
        /// Gets or sets the supported source types.
        /// </summary>
        public List<string> SupportedSourceTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported data formats.
        /// </summary>
        public List<string> SupportedDataFormats { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the supported schema formats.
        /// </summary>
        public List<SchemaFormat> SupportedSchemaFormats { get; set; } = new List<SchemaFormat>();

        /// <summary>
        /// Gets or sets a value indicating whether the importer supports validation.
        /// </summary>
        public bool SupportsValidation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the importer supports streaming.
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the importer supports batching.
        /// </summary>
        public bool SupportsBatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the importer supports filtering.
        /// </summary>
        public bool SupportsFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the importer supports transformation.
        /// </summary>
        public bool SupportsTransformation { get; set; }

        /// <summary>
        /// Gets or sets the maximum batch size supported by the importer.
        /// </summary>
        public int? MaxBatchSize { get; set; }
    }
}
