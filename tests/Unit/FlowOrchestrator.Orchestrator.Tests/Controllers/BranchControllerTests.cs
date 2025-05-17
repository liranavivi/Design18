using BranchExecutionContext = FlowOrchestrator.Orchestrator.BranchExecutionContext;
using BranchStatus = FlowOrchestrator.Orchestrator.BranchStatus;
using FlowOrchestrator.Orchestrator.Controllers;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Controllers
{
    /// <summary>
    /// Tests for the BranchController class.
    /// </summary>
    public class BranchControllerTests
    {
        private readonly Mock<ILogger<BranchController>> _loggerMock;
        private readonly Mock<BranchManagementService> _branchManagementServiceMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly BranchController _branchController;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchControllerTests"/> class.
        /// </summary>
        public BranchControllerTests()
        {
            _loggerMock = new Mock<ILogger<BranchController>>();
            _branchManagementServiceMock = new Mock<BranchManagementService>(
                new Mock<ILogger<BranchManagementService>>().Object,
                new Mock<IExecutionRepository>().Object,
                new Mock<TelemetryService>(new Mock<ILogger<TelemetryService>>().Object).Object);
            _executionRepositoryMock = new Mock<IExecutionRepository>();

            _branchController = new BranchController(
                _loggerMock.Object,
                _branchManagementServiceMock.Object,
                _executionRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that the controller returns a list of branches for an execution.
        /// </summary>
        [Fact]
        public async Task GetBranches_ReturnsExecutionBranches()
        {
            // Arrange
            var executionId = "exec-001";
            var branches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-001",
                    Status = BranchStatus.NEW
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    Status = BranchStatus.IN_PROGRESS
                }
            };

            _executionRepositoryMock.Setup(r => r.GetBranchesAsync(executionId))
                .ReturnsAsync(branches);

            // Act
            var result = await _branchController.GetBranches(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<BranchExecutionContext>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("branch-001", returnValue[0].BranchId);
            Assert.Equal("branch-002", returnValue[1].BranchId);
        }

        /// <summary>
        /// Tests that the controller returns a specific branch.
        /// </summary>
        [Fact]
        public async Task GetBranch_WithValidIds_ReturnsBranch()
        {
            // Arrange
            var executionId = "exec-001";
            var branchId = "branch-001";
            var branch = new BranchExecutionContext
            {
                ExecutionId = executionId,
                BranchId = branchId,
                Status = BranchStatus.NEW
            };

            _executionRepositoryMock.Setup(r => r.GetBranchContextAsync(executionId, branchId))
                .ReturnsAsync(branch);

            // Act
            var result = await _branchController.GetBranch(executionId, branchId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<BranchExecutionContext>(okResult.Value);
            Assert.Equal(executionId, returnValue.ExecutionId);
            Assert.Equal(branchId, returnValue.BranchId);
            Assert.Equal(BranchStatus.NEW, returnValue.Status);
        }

        /// <summary>
        /// Tests that the controller returns NotFound for a non-existent branch.
        /// </summary>
        [Fact]
        public async Task GetBranch_WithInvalidIds_ReturnsNotFound()
        {
            // Arrange
            var executionId = "exec-001";
            var branchId = "non-existent";

            _executionRepositoryMock.Setup(r => r.GetBranchContextAsync(executionId, branchId))
                .ReturnsAsync((BranchExecutionContext)null);

            // Act
            var result = await _branchController.GetBranch(executionId, branchId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that the controller updates branch status successfully.
        /// </summary>
        [Fact]
        public async Task UpdateBranchStatus_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var executionId = "exec-001";
            var branchId = "branch-001";
            var status = BranchStatus.IN_PROGRESS;

            _branchManagementServiceMock.Setup(s => s.UpdateBranchStatusAsync(executionId, branchId, status))
                .ReturnsAsync(true);

            // Act
            var result = await _branchController.UpdateBranchStatus(executionId, branchId, status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
        }

        /// <summary>
        /// Tests that the controller returns NotFound when updating a non-existent branch.
        /// </summary>
        [Fact]
        public async Task UpdateBranchStatus_WithInvalidIds_ReturnsNotFound()
        {
            // Arrange
            var executionId = "exec-001";
            var branchId = "non-existent";
            var status = BranchStatus.IN_PROGRESS;

            _branchManagementServiceMock.Setup(s => s.UpdateBranchStatusAsync(executionId, branchId, status))
                .ReturnsAsync(false);

            // Act
            var result = await _branchController.UpdateBranchStatus(executionId, branchId, status);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that the controller gets child branches successfully.
        /// </summary>
        [Fact]
        public async Task GetChildBranches_WithValidIds_ReturnsChildBranches()
        {
            // Arrange
            var executionId = "exec-001";
            var parentBranchId = "branch-001";
            var childBranches = new List<BranchExecutionContext>
            {
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-002",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.NEW
                },
                new BranchExecutionContext
                {
                    ExecutionId = executionId,
                    BranchId = "branch-003",
                    ParentBranchId = parentBranchId,
                    Status = BranchStatus.IN_PROGRESS
                }
            };

            _executionRepositoryMock.Setup(r => r.GetChildBranchesAsync(executionId, parentBranchId))
                .ReturnsAsync(childBranches);

            // Act
            var result = await _branchController.GetChildBranches(executionId, parentBranchId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<BranchExecutionContext>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("branch-002", returnValue[0].BranchId);
            Assert.Equal("branch-003", returnValue[1].BranchId);
            Assert.Equal(parentBranchId, returnValue[0].ParentBranchId);
            Assert.Equal(parentBranchId, returnValue[1].ParentBranchId);
        }
    }
}
