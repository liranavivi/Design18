using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Orchestrator.Controllers;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using FlowOrchestrator.Orchestrator.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Controllers
{
    /// <summary>
    /// Tests for the ExecutionController class.
    /// </summary>
    public class ExecutionControllerTests
    {
        private readonly Mock<ILogger<ExecutionController>> _loggerMock;
        private readonly Mock<OrchestratorService> _orchestratorServiceMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly ExecutionController _executionController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionControllerTests"/> class.
        /// </summary>
        public ExecutionControllerTests()
        {
            _loggerMock = new Mock<ILogger<ExecutionController>>();
            _orchestratorServiceMock = new Mock<OrchestratorService>(
                new Mock<ILogger<OrchestratorService>>().Object,
                new Mock<Infrastructure.Messaging.MassTransit.Abstractions.IMessageBus>().Object,
                new Mock<IExecutionRepository>().Object,
                MockErrorHandlingServiceFactory.Create(),
                MockTelemetryServiceFactory.Create());
            _executionRepositoryMock = new Mock<IExecutionRepository>();

            _executionController = new ExecutionController(
                _loggerMock.Object,
                _orchestratorServiceMock.Object,
                _executionRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that the controller returns a list of executions.
        /// </summary>
        [Fact]
        public async Task GetExecutions_ReturnsExecutionList()
        {
            // Arrange
            var executions = new List<ExecutionContext>
            {
                new ExecutionContext
                {
                    ExecutionId = "exec-001",
                    FlowId = "flow-001",
                    Status = ExecutionStatus.RUNNING
                },
                new ExecutionContext
                {
                    ExecutionId = "exec-002",
                    FlowId = "flow-002",
                    Status = ExecutionStatus.COMPLETED
                }
            };

            _executionRepositoryMock.Setup(r => r.GetExecutionsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(executions);

            // Act
            var result = await _executionController.GetExecutions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ExecutionContext>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("exec-001", returnValue[0].ExecutionId);
            Assert.Equal("exec-002", returnValue[1].ExecutionId);
        }

        /// <summary>
        /// Tests that the controller returns a specific execution.
        /// </summary>
        [Fact]
        public async Task GetExecution_WithValidId_ReturnsExecution()
        {
            // Arrange
            var executionId = "exec-001";
            var execution = new ExecutionContext
            {
                ExecutionId = executionId,
                FlowId = "flow-001",
                Status = ExecutionStatus.RUNNING
            };

            _executionRepositoryMock.Setup(r => r.GetExecutionContextAsync(executionId))
                .ReturnsAsync(execution);

            // Act
            var result = await _executionController.GetExecution(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ExecutionContext>(okResult.Value);
            Assert.Equal(executionId, returnValue.ExecutionId);
            Assert.Equal("flow-001", returnValue.FlowId);
            Assert.Equal(ExecutionStatus.RUNNING, returnValue.Status);
        }

        /// <summary>
        /// Tests that the controller returns NotFound for a non-existent execution.
        /// </summary>
        [Fact]
        public async Task GetExecution_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var executionId = "non-existent";

            _executionRepositoryMock.Setup(r => r.GetExecutionContextAsync(executionId))
                .ReturnsAsync((ExecutionContext)null);

            // Act
            var result = await _executionController.GetExecution(executionId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that the controller starts a flow execution successfully.
        /// </summary>
        [Fact]
        public async Task StartExecution_WithValidFlow_ReturnsCreatedResult()
        {
            // Arrange
            var flowId = "flow-001";
            var flowEntity = new Mock<IFlowEntity>();
            flowEntity.Setup(f => f.FlowId).Returns(flowId);
            flowEntity.Setup(f => f.Name).Returns("Test Flow");

            var executionParams = new Dictionary<string, object>
            {
                { "SourcePath", "C:\\Data\\Input" },
                { "DestinationPath", "C:\\Data\\Output" }
            };

            var executionContext = new ExecutionContext
            {
                ExecutionId = "exec-001",
                FlowId = flowId,
                Status = ExecutionStatus.RUNNING,
                Parameters = executionParams
            };

            _executionRepositoryMock.Setup(r => r.GetFlowEntityAsync(flowId))
                .ReturnsAsync(flowEntity.Object);
            _orchestratorServiceMock.Setup(s => s.StartFlowExecutionAsync(flowEntity.Object, executionParams))
                .ReturnsAsync(executionContext);

            // Act
            var result = await _executionController.StartExecution(flowId, executionParams);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ExecutionController.GetExecution), createdResult.ActionName);
            Assert.Equal("exec-001", createdResult.RouteValues["id"]);
            var returnValue = Assert.IsType<ExecutionContext>(createdResult.Value);
            Assert.Equal("exec-001", returnValue.ExecutionId);
            Assert.Equal(flowId, returnValue.FlowId);
            Assert.Equal(ExecutionStatus.RUNNING, returnValue.Status);
        }

        /// <summary>
        /// Tests that the controller returns NotFound when starting a non-existent flow.
        /// </summary>
        [Fact]
        public async Task StartExecution_WithInvalidFlow_ReturnsNotFound()
        {
            // Arrange
            var flowId = "non-existent";
            var executionParams = new Dictionary<string, object>();

            _executionRepositoryMock.Setup(r => r.GetFlowEntityAsync(flowId))
                .ReturnsAsync((IFlowEntity)null);

            // Act
            var result = await _executionController.StartExecution(flowId, executionParams);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that the controller cancels an execution successfully.
        /// </summary>
        [Fact]
        public async Task CancelExecution_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var executionId = "exec-001";
            var execution = new ExecutionContext
            {
                ExecutionId = executionId,
                FlowId = "flow-001",
                Status = ExecutionStatus.RUNNING
            };

            _executionRepositoryMock.Setup(r => r.GetExecutionContextAsync(executionId))
                .ReturnsAsync(execution);
            _orchestratorServiceMock.Setup(s => s.CancelExecutionAsync(executionId))
                .ReturnsAsync(true);

            // Act
            var result = await _executionController.CancelExecution(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
        }

        /// <summary>
        /// Tests that the controller returns NotFound when canceling a non-existent execution.
        /// </summary>
        [Fact]
        public async Task CancelExecution_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var executionId = "non-existent";

            _executionRepositoryMock.Setup(r => r.GetExecutionContextAsync(executionId))
                .ReturnsAsync((ExecutionContext)null);

            // Act
            var result = await _executionController.CancelExecution(executionId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
