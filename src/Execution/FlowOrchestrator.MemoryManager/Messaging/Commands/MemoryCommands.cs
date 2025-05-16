using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.MemoryManager.Messaging.Commands
{
    /// <summary>
    /// Command to allocate memory.
    /// </summary>
    public class AllocateMemoryCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flow identifier.
        /// </summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the step type.
        /// </summary>
        public string StepType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch path.
        /// </summary>
        public string BranchPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the step identifier.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional information.
        /// </summary>
        public string? AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live in seconds.
        /// </summary>
        public int? TimeToLiveSeconds { get; set; }

        /// <summary>
        /// Gets or sets the estimated data size in bytes.
        /// </summary>
        public long? EstimatedSizeBytes { get; set; }
    }

    /// <summary>
    /// Result of a memory allocation command.
    /// </summary>
    public class AllocateMemoryCommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the allocation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the memory address.
        /// </summary>
        public string MemoryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the allocation timestamp.
        /// </summary>
        public DateTime AllocationTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the expiration timestamp.
        /// </summary>
        public DateTime? ExpirationTimestamp { get; set; }
    }

    /// <summary>
    /// Command to deallocate memory.
    /// </summary>
    public class DeallocateMemoryCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the memory address to deallocate.
        /// </summary>
        public string MemoryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        public FlowOrchestrator.Abstractions.Common.ExecutionContext? Context { get; set; }
    }

    /// <summary>
    /// Result of a memory deallocation command.
    /// </summary>
    public class DeallocateMemoryCommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the deallocation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the memory address.
        /// </summary>
        public string MemoryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the deallocation timestamp.
        /// </summary>
        public DateTime DeallocationTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Command to clean up memory for an execution.
    /// </summary>
    public class CleanupExecutionMemoryCommand
    {
        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of a memory cleanup command.
    /// </summary>
    public class CleanupExecutionMemoryCommandResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the cleanup was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier.
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of memory addresses cleaned up.
        /// </summary>
        public int CleanedUpCount { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the cleanup timestamp.
        /// </summary>
        public DateTime CleanupTimestamp { get; set; } = DateTime.UtcNow;
    }
}
