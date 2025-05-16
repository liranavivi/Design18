using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.MemoryManager.Models
{
    /// <summary>
    /// Represents a memory allocation request.
    /// </summary>
    public class MemoryAllocationRequest
    {
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

        /// <summary>
        /// Gets or sets the access control list.
        /// </summary>
        public List<string>? AccessControlList { get; set; }
    }

    /// <summary>
    /// Represents a memory allocation result.
    /// </summary>
    public class MemoryAllocationResultInternal
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

    /// <summary>
    /// Represents a memory address with its components.
    /// </summary>
    public class MemoryAddress
    {
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
        /// Gets the full address string.
        /// </summary>
        public string FullAddress
        {
            get
            {
                var address = $"{ExecutionId}:{FlowId}:{StepType}:{BranchPath}:{StepId}:{DataType}";
                if (!string.IsNullOrEmpty(AdditionalInfo))
                {
                    address += $":{AdditionalInfo}";
                }
                return address;
            }
        }

        /// <summary>
        /// Parses a memory address string into a MemoryAddress object.
        /// </summary>
        /// <param name="address">The address string to parse.</param>
        /// <returns>A MemoryAddress object.</returns>
        public static MemoryAddress Parse(string address)
        {
            var components = address.Split(':');
            var result = new MemoryAddress();

            if (components.Length >= 6)
            {
                result.ExecutionId = components[0];
                result.FlowId = components[1];
                result.StepType = components[2];
                result.BranchPath = components[3];
                result.StepId = components[4];
                result.DataType = components[5];

                if (components.Length > 6)
                {
                    result.AdditionalInfo = components[6];
                }
            }
            else
            {
                throw new ArgumentException($"Invalid memory address format: {address}");
            }

            return result;
        }

        /// <summary>
        /// Tries to parse a memory address string into a MemoryAddress object.
        /// </summary>
        /// <param name="address">The address string to parse.</param>
        /// <param name="result">The parsed MemoryAddress object.</param>
        /// <returns>True if parsing was successful; otherwise, false.</returns>
        public static bool TryParse(string address, out MemoryAddress result)
        {
            result = new MemoryAddress();

            try
            {
                result = Parse(address);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
