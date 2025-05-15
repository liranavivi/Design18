using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Importers
{
    /// <summary>
    /// Represents a command to import data from a source.
    /// </summary>
    public class ImportCommand
    {
        /// <summary>
        /// Gets or sets the source entity identifier.
        /// </summary>
        public string SourceEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source entity version.
        /// </summary>
        public string SourceEntityVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        public string ImporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer service version.
        /// </summary>
        public string ImporterServiceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output location where the imported data should be stored.
        /// </summary>
        public string OutputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the import parameters.
        /// </summary>
        public ImportParameters Parameters { get; set; } = new ImportParameters();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public ExecutionContext Context { get; set; } = new ExecutionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommand"/> class.
        /// </summary>
        public ImportCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommand"/> class with the specified parameters.
        /// </summary>
        /// <param name="sourceEntityId">The source entity identifier.</param>
        /// <param name="sourceEntityVersion">The source entity version.</param>
        /// <param name="importerServiceId">The importer service identifier.</param>
        /// <param name="importerServiceVersion">The importer service version.</param>
        /// <param name="outputLocation">The output location.</param>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        public ImportCommand(
            string sourceEntityId,
            string sourceEntityVersion,
            string importerServiceId,
            string importerServiceVersion,
            string outputLocation,
            ImportParameters parameters,
            ExecutionContext context)
        {
            SourceEntityId = sourceEntityId;
            SourceEntityVersion = sourceEntityVersion;
            ImporterServiceId = importerServiceId;
            ImporterServiceVersion = importerServiceVersion;
            OutputLocation = outputLocation;
            Parameters = parameters;
            Context = context;
        }
    }

    /// <summary>
    /// Represents the result of an import command.
    /// </summary>
    public class ImportCommandResult
    {
        /// <summary>
        /// Gets or sets the import command that was executed.
        /// </summary>
        public ImportCommand Command { get; set; } = new ImportCommand();

        /// <summary>
        /// Gets or sets the import result.
        /// </summary>
        public ImportResult Result { get; set; } = new ImportResult();

        /// <summary>
        /// Gets or sets the timestamp when the command was completed.
        /// </summary>
        public DateTime CompletedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommandResult"/> class.
        /// </summary>
        public ImportCommandResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommandResult"/> class with the specified command and result.
        /// </summary>
        /// <param name="command">The import command that was executed.</param>
        /// <param name="result">The import result.</param>
        public ImportCommandResult(ImportCommand command, ImportResult result)
        {
            Command = command;
            Result = result;
        }
    }

    /// <summary>
    /// Represents an error that occurred while executing an import command.
    /// </summary>
    public class ImportCommandError
    {
        /// <summary>
        /// Gets or sets the import command that failed.
        /// </summary>
        public ImportCommand Command { get; set; } = new ImportCommand();

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError Error { get; set; } = new ExecutionError();

        /// <summary>
        /// Gets or sets the timestamp when the error occurred.
        /// </summary>
        public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommandError"/> class.
        /// </summary>
        public ImportCommandError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommandError"/> class with the specified command and error.
        /// </summary>
        /// <param name="command">The import command that failed.</param>
        /// <param name="error">The error information.</param>
        public ImportCommandError(ImportCommand command, ExecutionError error)
        {
            Command = command;
            Error = error;
        }
    }
}
