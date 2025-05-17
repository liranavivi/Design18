using FlowOrchestrator.Orchestrator.Controllers;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Controllers
{
    /// <summary>
    /// Tests for the MemoryController class.
    /// </summary>
    public class MemoryControllerTests
    {
        private readonly Mock<ILogger<MemoryController>> _loggerMock;
        private readonly Mock<MemoryAddressService> _memoryAddressServiceMock;
        private readonly MemoryController _memoryController;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryControllerTests"/> class.
        /// </summary>
        public MemoryControllerTests()
        {
            _loggerMock = new Mock<ILogger<MemoryController>>();
            _memoryAddressServiceMock = new Mock<MemoryAddressService>(
                new Mock<ILogger<MemoryAddressService>>().Object);

            _memoryController = new MemoryController(
                _loggerMock.Object,
                _memoryAddressServiceMock.Object);
        }

        /// <summary>
        /// Tests that the controller generates a memory address successfully.
        /// </summary>
        [Fact]
        public void GenerateMemoryAddress_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var executionId = "exec-001";
            var flowId = "flow-001";
            var stepType = "PROCESSOR";
            var branchPath = "main";
            var stepId = "processor-001";
            var dataType = "JSON";
            var address = "exec-001:flow-001:PROCESSOR:main:processor-001:JSON";

            _memoryAddressServiceMock.Setup(s => s.GenerateMemoryAddress(
                    executionId, flowId, stepType, branchPath, stepId, dataType, null))
                .Returns(address);

            // Act
            var result = _memoryController.GenerateMemoryAddress(
                executionId, flowId, stepType, branchPath, stepId, dataType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal(address, returnValue);
        }

        /// <summary>
        /// Tests that the controller generates a memory address with additional info successfully.
        /// </summary>
        [Fact]
        public void GenerateMemoryAddress_WithAdditionalInfo_ReturnsOkResult()
        {
            // Arrange
            var executionId = "exec-001";
            var flowId = "flow-001";
            var stepType = "PROCESSOR";
            var branchPath = "main";
            var stepId = "processor-001";
            var dataType = "JSON";
            var additionalInfo = "metadata";
            var address = "exec-001:flow-001:PROCESSOR:main:processor-001:JSON:metadata";

            _memoryAddressServiceMock.Setup(s => s.GenerateMemoryAddress(
                    executionId, flowId, stepType, branchPath, stepId, dataType, additionalInfo))
                .Returns(address);

            // Act
            var result = _memoryController.GenerateMemoryAddress(
                executionId, flowId, stepType, branchPath, stepId, dataType, additionalInfo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal(address, returnValue);
        }

        /// <summary>
        /// Tests that the controller handles exceptions when generating a memory address.
        /// </summary>
        [Fact]
        public void GenerateMemoryAddress_WithException_ReturnsBadRequest()
        {
            // Arrange
            var executionId = "exec-001";
            var flowId = "flow-001";
            var stepType = "PROCESSOR";
            var branchPath = "main";
            var stepId = "processor-001";
            var dataType = "JSON";

            _memoryAddressServiceMock.Setup(s => s.GenerateMemoryAddress(
                    executionId, flowId, stepType, branchPath, stepId, dataType, null))
                .Throws(new ArgumentException("Invalid parameters"));

            // Act
            var result = _memoryController.GenerateMemoryAddress(
                executionId, flowId, stepType, branchPath, stepId, dataType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(badRequestResult.Value);
            Assert.Equal("Invalid parameters", returnValue["error"]);
        }

        /// <summary>
        /// Tests that the controller parses a memory address successfully.
        /// </summary>
        [Fact]
        public void ParseMemoryAddress_WithValidAddress_ReturnsOkResult()
        {
            // Arrange
            var address = "exec-001:flow-001:PROCESSOR:main:processor-001:JSON";
            var components = new Dictionary<string, string>
            {
                { "executionId", "exec-001" },
                { "flowId", "flow-001" },
                { "stepType", "PROCESSOR" },
                { "branchPath", "main" },
                { "stepId", "processor-001" },
                { "dataType", "JSON" }
            };

            _memoryAddressServiceMock.Setup(s => s.ParseMemoryAddress(address))
                .Returns(components);

            // Act
            var result = _memoryController.ParseMemoryAddress(address);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal(6, returnValue.Count);
            Assert.Equal("exec-001", returnValue["executionId"]);
            Assert.Equal("flow-001", returnValue["flowId"]);
            Assert.Equal("PROCESSOR", returnValue["stepType"]);
            Assert.Equal("main", returnValue["branchPath"]);
            Assert.Equal("processor-001", returnValue["stepId"]);
            Assert.Equal("JSON", returnValue["dataType"]);
        }

        /// <summary>
        /// Tests that the controller handles exceptions when parsing a memory address.
        /// </summary>
        [Fact]
        public void ParseMemoryAddress_WithException_ReturnsBadRequest()
        {
            // Arrange
            var address = "invalid-address";

            _memoryAddressServiceMock.Setup(s => s.ParseMemoryAddress(address))
                .Throws(new ArgumentException("Invalid address format"));

            // Act
            var result = _memoryController.ParseMemoryAddress(address);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(badRequestResult.Value);
            Assert.Equal("Invalid address format", returnValue["error"]);
        }
    }
}
