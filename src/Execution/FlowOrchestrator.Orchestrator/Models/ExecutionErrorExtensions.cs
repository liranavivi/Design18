using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Orchestrator.Models
{
    /// <summary>
    /// Extension methods for ExecutionError.
    /// </summary>
    public static class ExecutionErrorExtensions
    {
        /// <summary>
        /// Gets the message from an execution error.
        /// </summary>
        /// <param name="error">The execution error.</param>
        /// <returns>The error message.</returns>
        public static string Message(this ExecutionError error)
        {
            // In a real implementation, this would extract the message from the error
            // For now, we'll just return a default message
            return error.ErrorCode ?? "Unknown error";
        }
    }
}
