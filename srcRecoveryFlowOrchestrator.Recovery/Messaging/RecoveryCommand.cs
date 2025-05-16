using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Recovery.Models;

namespace FlowOrchestrator.Recovery.Messaging
{
    /// <summary>
    /// Represents a recovery command.
    /// </summary>
    public class RecoveryCommand : ICommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the recovery identifier.
        /// </summary>
        public string RecoveryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the strategy name.
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error context.
        /// </summary>
        public ErrorContext ErrorContext { get; set; } = new ErrorContext();

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? ExecutionContext { get; set; }

        /// <summary>
        /// Gets or sets the strategy parameters.
        /// </summary>
        public Dictionary<string, object> StrategyParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the result of a recovery command.
    /// </summary>
    public class RecoveryCommandResult : ICommandResult
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
    }
}
