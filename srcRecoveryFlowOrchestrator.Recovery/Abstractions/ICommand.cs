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
        FlowOrchestrator.Abstractions.Common.ExecutionError? Error { get; set; }
    }
}
