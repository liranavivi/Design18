using System;

namespace FlowOrchestrator.Abstractions.Messaging.Messages
{
    /// <summary>
    /// Result of a memory allocation command.
    /// </summary>
    public class MemoryAllocationResult
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the allocation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the allocation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the memory address if the allocation was successful.
        /// </summary>
        public string MemoryAddress { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; }

        /// <summary>
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; }
    }
}
