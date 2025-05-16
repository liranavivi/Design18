using System;

namespace FlowOrchestrator.Abstractions.Messaging.Messages
{
    /// <summary>
    /// Command to allocate memory for a flow execution step.
    /// </summary>
    public class MemoryAllocationCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public Guid CommandId { get; set; } = Guid.NewGuid();

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

        /// <summary>
        /// Gets or sets the size of the memory to allocate in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the memory type.
        /// </summary>
        public string MemoryType { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the time-to-live in seconds.
        /// </summary>
        public int TimeToLiveSeconds { get; set; } = 3600;
    }
}
