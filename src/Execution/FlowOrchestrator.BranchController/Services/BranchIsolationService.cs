using FlowOrchestrator.BranchController.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlowOrchestrator.BranchController.Services
{
    /// <summary>
    /// Service for managing branch isolation.
    /// </summary>
    public class BranchIsolationService
    {
        private readonly ILogger<BranchIsolationService> _logger;
        private readonly BranchContextService _branchContextService;
        private readonly ConcurrentDictionary<string, HashSet<string>> _branchMemoryAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchIsolationService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="branchContextService">The branch context service.</param>
        public BranchIsolationService(
            ILogger<BranchIsolationService> logger,
            BranchContextService branchContextService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchContextService = branchContextService ?? throw new ArgumentNullException(nameof(branchContextService));
            _branchMemoryAddresses = new ConcurrentDictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// Registers a memory address for a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="memoryAddress">The memory address.</param>
        /// <returns>True if the memory address was registered, false otherwise.</returns>
        public bool RegisterMemoryAddress(string executionId, string branchId, string memoryAddress)
        {
            var branchContext = _branchContextService.GetBranch(executionId, branchId);
            if (branchContext == null)
            {
                _logger.LogWarning("Cannot register memory address for non-existent branch {BranchId}", branchId);
                return false;
            }

            var key = GetBranchKey(executionId, branchId);
            var addresses = _branchMemoryAddresses.GetOrAdd(key, _ => new HashSet<string>());
            
            lock (addresses)
            {
                addresses.Add(memoryAddress);
                branchContext.MemoryAddresses.Add(memoryAddress);
            }

            _logger.LogDebug("Registered memory address {MemoryAddress} for branch {BranchId}", memoryAddress, branchId);
            return true;
        }

        /// <summary>
        /// Validates that a memory address belongs to a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <param name="memoryAddress">The memory address.</param>
        /// <returns>True if the memory address belongs to the branch, false otherwise.</returns>
        public bool ValidateMemoryAddressBelongsToBranch(string executionId, string branchId, string memoryAddress)
        {
            var key = GetBranchKey(executionId, branchId);
            if (_branchMemoryAddresses.TryGetValue(key, out var addresses))
            {
                lock (addresses)
                {
                    return addresses.Contains(memoryAddress);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all memory addresses for a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The memory addresses.</returns>
        public IEnumerable<string> GetMemoryAddressesForBranch(string executionId, string branchId)
        {
            var key = GetBranchKey(executionId, branchId);
            if (_branchMemoryAddresses.TryGetValue(key, out var addresses))
            {
                lock (addresses)
                {
                    return addresses.ToList();
                }
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Cleans up memory addresses for a branch.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>True if the memory addresses were cleaned up, false otherwise.</returns>
        public bool CleanupMemoryAddresses(string executionId, string branchId)
        {
            var key = GetBranchKey(executionId, branchId);
            return _branchMemoryAddresses.TryRemove(key, out _);
        }

        /// <summary>
        /// Gets the branch key.
        /// </summary>
        /// <param name="executionId">The execution identifier.</param>
        /// <param name="branchId">The branch identifier.</param>
        /// <returns>The branch key.</returns>
        private static string GetBranchKey(string executionId, string branchId)
        {
            return $"{executionId}:{branchId}";
        }
    }
}
