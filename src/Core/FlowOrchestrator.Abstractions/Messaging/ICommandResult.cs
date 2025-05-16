using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Messaging
{
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
