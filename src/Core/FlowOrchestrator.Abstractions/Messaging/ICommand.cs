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
}
