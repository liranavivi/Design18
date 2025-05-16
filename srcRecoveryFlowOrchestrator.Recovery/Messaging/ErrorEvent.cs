using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Recovery.Models;

namespace FlowOrchestrator.Recovery.Messaging
{
    /// <summary>
    /// Represents an error event.
    /// </summary>
    public class ErrorEvent : ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the error context.
        /// </summary>
        public ErrorContext ErrorContext { get; set; } = new ErrorContext();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? ExecutionContext { get; set; }
    }

    /// <summary>
    /// Represents the result of an error event.
    /// </summary>
    public class ErrorEventResult : ICommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets a value indicating whether the command was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error information.
        /// </summary>
        public ExecutionError? Error { get; set; }

        /// <summary>
        /// Gets or sets the recovery result.
        /// </summary>
        public RecoveryResult? RecoveryResult { get; set; }

        /// <summary>
        /// Gets or sets the recovery action.
        /// </summary>
        public RecoveryAction RecoveryAction { get; set; }

        /// <summary>
        /// Gets or sets the next retry delay.
        /// </summary>
        public TimeSpan? NextRetryDelay { get; set; }
    }
}
