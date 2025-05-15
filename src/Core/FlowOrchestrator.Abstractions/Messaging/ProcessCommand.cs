using FlowOrchestrator.Abstractions.Services;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;

namespace FlowOrchestrator.Abstractions.Messaging
{
    /// <summary>
    /// Represents a command to process data.
    /// </summary>
    public class ProcessCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

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
        /// Gets or sets the process parameters.
        /// </summary>
        public ProcessParameters Parameters { get; set; } = new ProcessParameters();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public ExecutionContext Context { get; set; } = new ExecutionContext();
    }
}
