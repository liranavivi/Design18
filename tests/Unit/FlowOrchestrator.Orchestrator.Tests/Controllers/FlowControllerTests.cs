using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Orchestrator.Controllers;
using FlowOrchestrator.Orchestrator.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Controllers
{
    /// <summary>
    /// Tests for the FlowController class.
    /// </summary>
    public class FlowControllerTests
    {
        private readonly Mock<ILogger<FlowController>> _loggerMock;
        private readonly Mock<IFlowRepository> _flowRepositoryMock;
        private readonly FlowController _flowController;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowControllerTests"/> class.
        /// </summary>
        public FlowControllerTests()
        {
            _loggerMock = new Mock<ILogger<FlowController>>();
            _flowRepositoryMock = new Mock<IFlowRepository>();

            _flowController = new FlowController(
                _loggerMock.Object,
                _flowRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that the controller returns a list of flows.
        /// </summary>
        [Fact]
        public async Task GetFlows_ReturnsFlowList()
        {
            // Arrange
            var flows = new List<IFlowEntity>
            {
                CreateMockFlowEntity("flow-001", "Flow 1"),
                CreateMockFlowEntity("flow-002", "Flow 2")
            };

            _flowRepositoryMock.Setup(r => r.GetFlowsAsync())
                .ReturnsAsync(flows);

            // Act
            var result = await _flowController.GetFlows();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<IFlowEntity>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("flow-001", returnValue[0].FlowId);
            Assert.Equal("flow-002", returnValue[1].FlowId);
        }

        /// <summary>
        /// Tests that the controller returns a specific flow.
        /// </summary>
        [Fact]
        public async Task GetFlow_WithValidId_ReturnsFlow()
        {
            // Arrange
            var flowId = "flow-001";
            var flow = CreateMockFlowEntity(flowId, "Flow 1");

            _flowRepositoryMock.Setup(r => r.GetFlowAsync(flowId))
                .ReturnsAsync(flow);

            // Act
            var result = await _flowController.GetFlow(flowId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<IFlowEntity>(okResult.Value);
            Assert.Equal(flowId, returnValue.FlowId);
            Assert.Equal("Flow 1", returnValue.Name);
        }

        /// <summary>
        /// Tests that the controller returns NotFound for a non-existent flow.
        /// </summary>
        [Fact]
        public async Task GetFlow_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var flowId = "non-existent";

            _flowRepositoryMock.Setup(r => r.GetFlowAsync(flowId))
                .ReturnsAsync((IFlowEntity)null);

            // Act
            var result = await _flowController.GetFlow(flowId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that the controller creates a flow successfully.
        /// </summary>
        [Fact]
        public async Task CreateFlow_WithValidFlow_ReturnsCreatedResult()
        {
            // Arrange
            var flow = CreateMockFlowEntity("flow-001", "Flow 1");

            _flowRepositoryMock.Setup(r => r.SaveFlowAsync(flow))
                .ReturnsAsync(true);

            // Act
            var result = await _flowController.CreateFlow(flow);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(FlowController.GetFlow), createdResult.ActionName);
            Assert.Equal("flow-001", createdResult.RouteValues["id"]);
            var returnValue = Assert.IsType<IFlowEntity>(createdResult.Value);
            Assert.Equal("flow-001", returnValue.FlowId);
            Assert.Equal("Flow 1", returnValue.Name);
        }

        /// <summary>
        /// Tests that the controller returns BadRequest when creating a flow with an existing ID.
        /// </summary>
        [Fact]
        public async Task CreateFlow_WithExistingId_ReturnsBadRequest()
        {
            // Arrange
            var flow = CreateMockFlowEntity("flow-001", "Flow 1");

            _flowRepositoryMock.Setup(r => r.FlowExistsAsync("flow-001"))
                .ReturnsAsync(true);

            // Act
            var result = await _flowController.CreateFlow(flow);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that the controller updates a flow successfully.
        /// </summary>
        [Fact]
        public async Task UpdateFlow_WithValidFlow_ReturnsOkResult()
        {
            // Arrange
            var flowId = "flow-001";
            var flow = CreateMockFlowEntity(flowId, "Updated Flow 1");

            _flowRepositoryMock.Setup(r => r.FlowExistsAsync(flowId))
                .ReturnsAsync(true);
            _flowRepositoryMock.Setup(r => r.UpdateFlowAsync(flow))
                .ReturnsAsync(true);

            // Act
            var result = await _flowController.UpdateFlow(flowId, flow);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<IFlowEntity>(okResult.Value);
            Assert.Equal(flowId, returnValue.FlowId);
            Assert.Equal("Updated Flow 1", returnValue.Name);
        }

        /// <summary>
        /// Tests that the controller returns NotFound when updating a non-existent flow.
        /// </summary>
        [Fact]
        public async Task UpdateFlow_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var flowId = "non-existent";
            var flow = CreateMockFlowEntity(flowId, "Flow 1");

            _flowRepositoryMock.Setup(r => r.FlowExistsAsync(flowId))
                .ReturnsAsync(false);

            // Act
            var result = await _flowController.UpdateFlow(flowId, flow);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that the controller deletes a flow successfully.
        /// </summary>
        [Fact]
        public async Task DeleteFlow_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var flowId = "flow-001";

            _flowRepositoryMock.Setup(r => r.FlowExistsAsync(flowId))
                .ReturnsAsync(true);
            _flowRepositoryMock.Setup(r => r.DeleteFlowAsync(flowId))
                .ReturnsAsync(true);

            // Act
            var result = await _flowController.DeleteFlow(flowId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
        }

        /// <summary>
        /// Tests that the controller returns NotFound when deleting a non-existent flow.
        /// </summary>
        [Fact]
        public async Task DeleteFlow_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var flowId = "non-existent";

            _flowRepositoryMock.Setup(r => r.FlowExistsAsync(flowId))
                .ReturnsAsync(false);

            // Act
            var result = await _flowController.DeleteFlow(flowId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Creates a mock flow entity.
        /// </summary>
        /// <param name="flowId">The flow identifier.</param>
        /// <param name="name">The flow name.</param>
        /// <returns>A mock flow entity.</returns>
        private IFlowEntity CreateMockFlowEntity(string flowId, string name)
        {
            var flowMock = new Mock<IFlowEntity>();
            flowMock.Setup(f => f.FlowId).Returns(flowId);
            flowMock.Setup(f => f.Name).Returns(name);
            return flowMock.Object;
        }
    }
}
