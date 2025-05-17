using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using BranchExecutionContext = FlowOrchestrator.Orchestrator.BranchExecutionContext;
using BranchStatus = FlowOrchestrator.Orchestrator.BranchStatus;
using MergePolicy = FlowOrchestrator.Orchestrator.MergePolicy;

namespace FlowOrchestrator.Orchestrator.Tests.Services
{
    /// <summary>
    /// Tests for the MergeStrategyService class.
    /// </summary>
    public class MergeStrategyServiceTests
    {
        private readonly Mock<ILogger<MergeStrategyService>> _loggerMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly MergeStrategyService _mergeStrategyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeStrategyServiceTests"/> class.
        /// </summary>
        public MergeStrategyServiceTests()
        {
            _loggerMock = new Mock<ILogger<MergeStrategyService>>();
            _executionRepositoryMock = new Mock<IExecutionRepository>();
            _mergeStrategyService = new MergeStrategyService(_loggerMock.Object, _executionRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that the service determines if a merge is needed when all branches are complete.
        /// </summary>
        [Fact]
        public async Task IsMergeNeededAsync_WithAllBranchesComplete_ReturnsTrue()
        {
            // Arrange
            var executionId = "exec-001";
            var parentBranchId = "branch-001";
            var mergePolicy = MergePolicy.ALL_BRANCHES_COMPLETE;

            var branches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.COMPLETED
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-003",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.COMPLETED
                }
            };

            _executionRepositoryMock.Setup(r => r.GetChildBranchesAsync(executionId, parentBranchId))
                .ReturnsAsync(branches);

            // Act
            var result = await _mergeStrategyService.IsMergeNeededAsync(executionId, parentBranchId, mergePolicy);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the service determines if a merge is not needed when not all branches are complete.
        /// </summary>
        [Fact]
        public async Task IsMergeNeededAsync_WithNotAllBranchesComplete_ReturnsFalse()
        {
            // Arrange
            var executionId = "exec-001";
            var parentBranchId = "branch-001";
            var mergePolicy = MergePolicy.ALL_BRANCHES_COMPLETE;

            var branches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.COMPLETED
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-003",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.PROCESSING
                }
            };

            _executionRepositoryMock.Setup(r => r.GetChildBranchesAsync(executionId, parentBranchId))
                .ReturnsAsync(branches);

            // Act
            var result = await _mergeStrategyService.IsMergeNeededAsync(executionId, parentBranchId, mergePolicy);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the service determines if a merge is needed when any branch is complete.
        /// </summary>
        [Fact]
        public async Task IsMergeNeededAsync_WithAnyBranchComplete_ReturnsTrue()
        {
            // Arrange
            var executionId = "exec-001";
            var parentBranchId = "branch-001";
            var mergePolicy = MergePolicy.ANY_BRANCH_COMPLETE;

            var branches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.COMPLETED
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-003",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.PROCESSING
                }
            };

            _executionRepositoryMock.Setup(r => r.GetChildBranchesAsync(executionId, parentBranchId))
                .ReturnsAsync(branches);

            // Act
            var result = await _mergeStrategyService.IsMergeNeededAsync(executionId, parentBranchId, mergePolicy);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the service determines if a merge is not needed when no branches are complete.
        /// </summary>
        [Fact]
        public async Task IsMergeNeededAsync_WithNoBranchesComplete_ReturnsFalse()
        {
            // Arrange
            var executionId = "exec-001";
            var parentBranchId = "branch-001";
            var mergePolicy = MergePolicy.ANY_BRANCH_COMPLETE;

            var branches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.PROCESSING
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-003",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.PROCESSING
                }
            };

            _executionRepositoryMock.Setup(r => r.GetChildBranchesAsync(executionId, parentBranchId))
                .ReturnsAsync(branches);

            // Act
            var result = await _mergeStrategyService.IsMergeNeededAsync(executionId, parentBranchId, mergePolicy);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the service selects the appropriate merge strategy based on the exporter capabilities.
        /// </summary>
        [Fact]
        public void SelectMergeStrategy_WithValidExporterCapabilities_ReturnsCorrectStrategy()
        {
            // Arrange
            var exporterCapabilities = new Dictionary<string, object>
            {
                { "SupportedMergeStrategies", new List<string> { "APPEND", "REPLACE" } }
            };

            // Act
            var result = _mergeStrategyService.SelectMergeStrategy(exporterCapabilities, "APPEND");

            // Assert
            Assert.Equal("APPEND", result);
        }

        /// <summary>
        /// Tests that the service falls back to the default merge strategy when the requested strategy is not supported.
        /// </summary>
        [Fact]
        public void SelectMergeStrategy_WithUnsupportedStrategy_ReturnsDefaultStrategy()
        {
            // Arrange
            var exporterCapabilities = new Dictionary<string, object>
            {
                { "SupportedMergeStrategies", new List<string> { "APPEND", "REPLACE" } }
            };

            // Act
            var result = _mergeStrategyService.SelectMergeStrategy(exporterCapabilities, "MERGE");

            // Assert
            Assert.Equal("APPEND", result); // Default strategy
        }
    }
}
