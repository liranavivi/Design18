using System;
using System.Collections.Generic;

namespace FlowOrchestrator.MemoryManager.Models
{
    /// <summary>
    /// Represents metadata for a memory allocation.
    /// </summary>
    public class MemoryMetadata
    {
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

        /// <summary>
        /// Gets or sets the memory metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
