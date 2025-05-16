using FlowOrchestrator.Abstractions.Common;

// Define interfaces locally since we don't have access to the actual interfaces
namespace FlowOrchestrator.Abstractions.Messaging
{
    /// <summary>
    /// Interface for commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        string CommandId { get; set; }
    }

    /// <summary>
    /// Interface for command results.
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        string ResultId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command was successful.
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        ExecutionError? Error { get; set; }
    }
}

namespace FlowOrchestrator.Management.Scheduling.Messaging.Commands
{
    /// <summary>
    /// Command to trigger a flow execution.
    /// </summary>
    public class TriggerFlowCommand : FlowOrchestrator.Abstractions.Messaging.ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow entity identifier.
        /// </summary>
        public string FlowEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow entity version.
        /// </summary>
        public string FlowEntityVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scheduled time.
        /// </summary>
        public DateTime ScheduledTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the execution parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext Context { get; set; } = new FlowOrchestrator.Abstractions.Common.ExecutionContext();
    }
}

namespace FlowOrchestrator.Abstractions.Messaging
{
    /// <summary>
    /// Command to process data.
    /// </summary>
    public class ProcessCommand : FlowOrchestrator.Abstractions.Messaging.ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor service identifier.
        /// </summary>
        public string ProcessorServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor service version.
        /// </summary>
        public string ProcessorServiceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input location.
        /// </summary>
        public string InputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output location.
        /// </summary>
        public string OutputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext Context { get; set; } = new FlowOrchestrator.Abstractions.Common.ExecutionContext();

        /// <summary>
        /// Gets or sets the process parameters.
        /// </summary>
        public ProcessParameters Parameters { get; set; } = new ProcessParameters();
    }

    /// <summary>
    /// Process parameters.
    /// </summary>
    public class ProcessParameters
    {
        /// <summary>
        /// Gets or sets the schema identifier.
        /// </summary>
        public string SchemaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        public string SchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transformation identifier.
        /// </summary>
        public string TransformationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transformation version.
        /// </summary>
        public string TransformationVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional parameters.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Result of a process command.
    /// </summary>
    public class ProcessCommandResult : FlowOrchestrator.Abstractions.Messaging.ICommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ProcessCommand Command { get; set; } = new ProcessCommand();

        /// <summary>
        /// Gets or sets a value indicating whether the command was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the result metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Command to import data.
    /// </summary>
    public class ImportCommand : FlowOrchestrator.Abstractions.Messaging.ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer service identifier.
        /// </summary>
        public string ImporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the importer service version.
        /// </summary>
        public string ImporterServiceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output location.
        /// </summary>
        public string OutputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext Context { get; set; } = new FlowOrchestrator.Abstractions.Common.ExecutionContext();

        /// <summary>
        /// Gets or sets the import parameters.
        /// </summary>
        public ImportParameters Parameters { get; set; } = new ImportParameters();
    }

    /// <summary>
    /// Import parameters.
    /// </summary>
    public class ImportParameters
    {
        /// <summary>
        /// Gets or sets the source location.
        /// </summary>
        public string SourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional parameters.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Result of an import command.
    /// </summary>
    public class ImportCommandResult : FlowOrchestrator.Abstractions.Messaging.ICommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ImportCommand Command { get; set; } = new ImportCommand();

        /// <summary>
        /// Gets or sets a value indicating whether the command was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the result metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Command to export data.
    /// </summary>
    public class ExportCommand : FlowOrchestrator.Abstractions.Messaging.ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter service identifier.
        /// </summary>
        public string ExporterServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exporter service version.
        /// </summary>
        public string ExporterServiceVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input location.
        /// </summary>
        public string InputLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext Context { get; set; } = new FlowOrchestrator.Abstractions.Common.ExecutionContext();

        /// <summary>
        /// Gets or sets the export parameters.
        /// </summary>
        public ExportParameters Parameters { get; set; } = new ExportParameters();
    }

    /// <summary>
    /// Export parameters.
    /// </summary>
    public class ExportParameters
    {
        /// <summary>
        /// Gets or sets the destination location.
        /// </summary>
        public string DestinationLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination type.
        /// </summary>
        public string DestinationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional parameters.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Result of an export command.
    /// </summary>
    public class ExportCommandResult : FlowOrchestrator.Abstractions.Messaging.ICommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ExportCommand Command { get; set; } = new ExportCommand();

        /// <summary>
        /// Gets or sets a value indicating whether the command was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the result metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
