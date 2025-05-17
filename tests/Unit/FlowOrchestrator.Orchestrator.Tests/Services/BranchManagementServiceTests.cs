using FlowOrchestrator.Orchestrator;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using FlowOrchestrator.Orchestrator.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using BranchExecutionContext = FlowOrchestrator.Orchestrator.BranchExecutionContext;

namespace FlowOrchestrator.Orchestrator.Tests.Services
{
    /// <summary>
    /// Tests for the BranchManagementService class.
    /// </summary>
    public class BranchManagementServiceTests
    {
        private readonly Mock<ILogger<BranchManagementService>> _loggerMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly TelemetryService _telemetryService;
        private readonly BranchManagementService _branchManagementService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchManagementServiceTests"/> class.
        /// </summary>
        public BranchManagementServiceTests()
        {
            _loggerMock = new Mock<ILogger<BranchManagementService>>();
            _executionRepositoryMock = new Mock<IExecutionRepository>();
            _telemetryService = MockTelemetryServiceFactory.Create();

            _branchManagementService = new BranchManagementService(
                _loggerMock.Object,
                _executionRepositoryMock.Object,
                _telemetryService);
        }

        /// <summary>
        /// Tests that the service creates a branch successfully.
        /// </summary>
        [Fact]
        public async Task CreateBranchAsync_WithValidParameters_ReturnsBranchContext()
        {
            // Arrange
            var executionContext = new ExecutionContext
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                Status = ExecutionStatus.STARTED,
                Parameters = new Dictionary<string, object>
                {
                    { "SourcePath", "C:\\Data\\Input" },
                    { "DestinationPath", "C:\\Data\\Output" }
                }
            };

            _executionRepositoryMock.Setup(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _branchManagementService.CreateBranchAsync(executionContext, "branch-001", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("exec-001", result.ExecutionId);
            Assert.Equal("branch-001", result.BranchId);
            Assert.Null(result.ParentBranchId);
            Assert.Equal(BranchStatus.NEW, result.Status);
            Assert.NotNull(result.Parameters);
            Assert.Equal(executionContext.Parameters, result.Parameters);

            _executionRepositoryMock.Verify(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()), Times.Once);
            // We can't verify the call to RecordBranchExecutionStart since we're using a concrete implementation
        }

        /// <summary>
        /// Tests that the service creates a child branch successfully.
        /// </summary>
        [Fact]
        public async Task CreateBranchAsync_WithParentBranch_ReturnsChildBranchContext()
        {
            // Arrange
            var executionContext = new ExecutionContext
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                Status = ExecutionStatus.STARTED,
                Parameters = new Dictionary<string, object>
                {
                    { "SourcePath", "C:\\Data\\Input" },
                    { "DestinationPath", "C:\\Data\\Output" }
                }
            };

            _executionRepositoryMock.Setup(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _branchManagementService.CreateBranchAsync(executionContext, "branch-002", "branch-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("exec-001", result.ExecutionId);
            Assert.Equal("branch-002", result.BranchId);
            Assert.Equal("branch-001", result.ParentBranchId);
            Assert.Equal(BranchStatus.NEW, result.Status);
            Assert.NotNull(result.Parameters);
            Assert.Equal(executionContext.Parameters, result.Parameters);

            _executionRepositoryMock.Verify(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()), Times.Once);
        }

        /// <summary>
        /// Tests that the service updates branch status successfully.
        /// </summary>
        [Fact]
        public async Task UpdateBranchStatusAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var branchContext = new BranchExecutionContext
            {
                ExecutionId = "exec-001",
                BranchId = "branch-001",
                Status = BranchStatus.NEW,
                StartTimestamp = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>()
            };

            _executionRepositoryMock.Setup(r => r.GetBranchContextAsync("exec-001", "branch-001"))
                .ReturnsAsync(branchContext);
            _executionRepositoryMock.Setup(r => r.UpdateBranchContextAsync(It.IsAny<BranchExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _branchManagementService.UpdateBranchStatusAsync("exec-001", "branch-001", BranchStatus.IN_PROGRESS);

            // Assert
            Assert.True(result);
            Assert.Equal(BranchStatus.IN_PROGRESS, branchContext.Status);
            Assert.Null(branchContext.EndTimestamp);

            _executionRepositoryMock.Verify(r => r.UpdateBranchContextAsync(It.IsAny<BranchExecutionContext>()), Times.Once);
        }

        /// <summary>
        /// Tests that the service updates branch status to completed and sets end timestamp.
        /// </summary>
        [Fact]
        public async Task UpdateBranchStatusAsync_WithCompletedStatus_SetsEndTimestamp()
        {
            // Arrange
            var branchContext = new BranchExecutionContext
            {
                ExecutionId = "exec-001",
                BranchId = "branch-001",
                Status = BranchStatus.IN_PROGRESS,
                StartTimestamp = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>()
            };

            _executionRepositoryMock.Setup(r => r.GetBranchContextAsync("exec-001", "branch-001"))
                .ReturnsAsync(branchContext);
            _executionRepositoryMock.Setup(r => r.UpdateBranchContextAsync(It.IsAny<BranchExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _branchManagementService.UpdateBranchStatusAsync("exec-001", "branch-001", BranchStatus.COMPLETED);

            // Assert
            Assert.True(result);
            Assert.Equal(BranchStatus.COMPLETED, branchContext.Status);
            Assert.NotNull(branchContext.EndTimestamp);

            _executionRepositoryMock.Verify(r => r.UpdateBranchContextAsync(It.IsAny<BranchExecutionContext>()), Times.Once);
            // We can't verify the call to RecordBranchExecutionEnd since we're using a concrete implementation
        }

        /// <summary>
        /// Tests that the service returns false when branch not found.
        /// </summary>
        [Fact]
        public async Task UpdateBranchStatusAsync_WithNonExistentBranch_ReturnsFalse()
        {
            // Arrange
            _executionRepositoryMock.Setup(r => r.GetBranchContextAsync("exec-001", "branch-001"))
                .ReturnsAsync((BranchExecutionContext)null);

            // Act
            var result = await _branchManagementService.UpdateBranchStatusAsync("exec-001", "branch-001", BranchStatus.IN_PROGRESS);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Branch branch-001 not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
