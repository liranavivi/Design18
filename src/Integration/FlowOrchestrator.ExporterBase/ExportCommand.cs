using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Integration.Exporters
{
    /// <summary>
    /// Represents a command to export data to a destination.
    /// </summary>
    public class ExportCommand
    {
        /// <summary>
        /// Gets or sets the destination entity identifier.
        /// </summary>
        public string DestinationEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination entity version.
        /// </summary>
        public string DestinationEntityVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter service identifier.
        /// </summary>
        public string ExporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter service version.
        /// </summary>
        public string ExporterServiceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input location where the data to export is stored.
        /// </summary>
        public string InputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the export parameters.
        /// </summary>
        public ExportParameters Parameters { get; set; } = new ExportParameters();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public ExecutionContext Context { get; set; } = new ExecutionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommand"/> class.
        /// </summary>
        public ExportCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommand"/> class with the specified parameters.
        /// </summary>
        /// <param name="destinationEntityId">The destination entity identifier.</param>
        /// <param name="destinationEntityVersion">The destination entity version.</param>
        /// <param name="exporterServiceId">The exporter service identifier.</param>
        /// <param name="exporterServiceVersion">The exporter service version.</param>
        /// <param name="inputLocation">The input location.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        public ExportCommand(
            string destinationEntityId,
            string destinationEntityVersion,
            string exporterServiceId,
            string exporterServiceVersion,
            string inputLocation,
            ExportParameters parameters,
            ExecutionContext context)
        {
            DestinationEntityId = destinationEntityId;
            DestinationEntityVersion = destinationEntityVersion;
            ExporterServiceId = exporterServiceId;
            ExporterServiceVersion = exporterServiceVersion;
            InputLocation = inputLocation;
            Parameters = parameters;
            Context = context;
        }
    }

    /// <summary>
    /// Represents the result of an export command.
    /// </summary>
    public class ExportCommandResult
    {
        /// <summary>
        /// Gets or sets the export command that was executed.
        /// </summary>
        public ExportCommand Command { get; set; } = new ExportCommand();

        /// <summary>
        /// Gets or sets the export result.
        /// </summary>
        public ExportResult Result { get; set; } = new ExportResult();

        /// <summary>
        /// Gets or sets the timestamp when the command was completed.
        /// </summary>
        public DateTime CompletedTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommandResult"/> class.
        /// </summary>
        public ExportCommandResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommandResult"/> class with the specified command and result.
        /// </summary>
        /// <param name="command">The export command that was executed.</param>
        /// <param name="result">The export result.</param>
        public ExportCommandResult(ExportCommand command, ExportResult result)
        {
            Command = command;
            Result = result;
        }
    }

    /// <summary>
    /// Represents an error that occurred while executing an export command.
    /// </summary>
    public class ExportCommandError
    {
        /// <summary>
        /// Gets or sets the export command that failed.
        /// </summary>
        public ExportCommand Command { get; set; } = new ExportCommand();

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError Error { get; set; } = new ExecutionError();

        /// <summary>
        /// Gets or sets the timestamp when the error occurred.
        /// </summary>
        public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommandError"/> class.
        /// </summary>
        public ExportCommandError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommandError"/> class with the specified command and error.
        /// </summary>
        /// <param name="command">The export command that failed.</param>
        /// <param name="error">The error information.</param>
        public ExportCommandError(ExportCommand command, ExecutionError error)
        {
            Command = command;
            Error = error;
        }
    }
}
