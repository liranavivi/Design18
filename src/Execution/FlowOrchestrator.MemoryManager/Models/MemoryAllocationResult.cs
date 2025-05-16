namespace FlowOrchestrator.MemoryManager.Models
{
    /// <summary>
    /// Represents the result of a memory allocation request.
    /// </summary>
    public class MemoryAllocationResult
    {
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
        /// Gets or sets the size of the allocated memory in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the memory type.
        /// </summary>
        public string MemoryType { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live in seconds.
        /// </summary>
        public int TimeToLiveSeconds { get; set; }
    }
}
