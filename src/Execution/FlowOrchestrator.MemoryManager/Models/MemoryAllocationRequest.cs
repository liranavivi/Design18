using FlowOrchestrator.Abstractions.Common;
using System.Threading;

namespace FlowOrchestrator.MemoryManager.Models
{
    /// <summary>
    /// Represents a request to allocate memory.
    /// </summary>
    public class MemoryAllocationRequestLegacy
    {
        /// <summary>
        /// Gets or sets the size of the memory to allocate in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch identifier.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the memory type.
        /// </summary>
        public string MemoryType { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the time-to-live in seconds.
        /// </summary>
        public int TimeToLiveSeconds { get; set; } = 3600;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext Context { get; set; } = new FlowOrchestrator.Abstractions.Common.ExecutionContext();
    }
}
