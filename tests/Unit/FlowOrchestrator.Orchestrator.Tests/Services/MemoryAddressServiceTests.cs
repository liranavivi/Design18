using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Services
{
    /// <summary>
    /// Tests for the MemoryAddressService class.
    /// </summary>
    public class MemoryAddressServiceTests
    {
        private readonly Mock<ILogger<MemoryAddressService>> _loggerMock;
        private readonly MemoryAddressService _memoryAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAddressServiceTests"/> class.
        /// </summary>
        public MemoryAddressServiceTests()
        {
            _loggerMock = new Mock<ILogger<MemoryAddressService>>();
            _memoryAddressService = new MemoryAddressService(_loggerMock.Object);
        }

        /// <summary>
        /// Tests that the service generates a memory address correctly.
        /// </summary>
        [Fact]
        public void GenerateMemoryAddress_WithValidRequest_ReturnsCorrectAddress()
        {
            // Arrange
            var request = new MemoryAllocationRequest
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                StepType = "PROCESSOR",
                BranchPath = "main",
                StepId = "processor-001",
                DataType = "JSON"
            };

            // Act
            var result = _memoryAddressService.GenerateMemoryAddress(
                request.ExecutionId,
                request.FlowId,
                request.StepType,
                request.BranchPath,
                request.StepId,
                request.DataType);

            // Assert
            Assert.Equal("exec-001:flow-001:PROCESSOR:main:processor-001:JSON", result);
        }

        /// <summary>
        /// Tests that the service generates a memory address with additional info correctly.
        /// </summary>
        [Fact]
        public void GenerateMemoryAddress_WithAdditionalInfo_ReturnsCorrectAddress()
        {
            // Arrange
            var request = new MemoryAllocationRequest
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                StepType = "PROCESSOR",
                BranchPath = "main",
                StepId = "processor-001",
                DataType = "JSON",
                AdditionalInfo = "metadata"
            };

            // Act
            var result = _memoryAddressService.GenerateMemoryAddress(
                request.ExecutionId,
                request.FlowId,
                request.StepType,
                request.BranchPath,
                request.StepId,
                request.DataType,
                request.AdditionalInfo);

            // Assert
            Assert.Equal("exec-001:flow-001:PROCESSOR:main:processor-001:JSON:metadata", result);
        }

        /// <summary>
        /// Tests that the service parses a memory address correctly.
        /// </summary>
        [Fact]
        public void ParseMemoryAddress_WithValidAddress_ReturnsCorrectComponents()
        {
            // Arrange
            var address = "exec-001:flow-001:PROCESSOR:main:processor-001:JSON";

            // Act
            var result = _memoryAddressService.ParseMemoryAddress(address);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("exec-001", result["executionId"]);
            Assert.Equal("flow-001", result["flowId"]);
            Assert.Equal("PROCESSOR", result["stepType"]);
            Assert.Equal("main", result["branchPath"]);
            Assert.Equal("processor-001", result["stepId"]);
            Assert.Equal("JSON", result["dataType"]);
            Assert.False(result.ContainsKey("additionalInfo"));
        }

        /// <summary>
        /// Tests that the service parses a memory address with additional info correctly.
        /// </summary>
        [Fact]
        public void ParseMemoryAddress_WithAdditionalInfo_ReturnsCorrectComponents()
        {
            // Arrange
            var address = "exec-001:flow-001:PROCESSOR:main:processor-001:JSON:metadata";

            // Act
            var result = _memoryAddressService.ParseMemoryAddress(address);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("exec-001", result["executionId"]);
            Assert.Equal("flow-001", result["flowId"]);
            Assert.Equal("PROCESSOR", result["stepType"]);
            Assert.Equal("main", result["branchPath"]);
            Assert.Equal("processor-001", result["stepId"]);
            Assert.Equal("JSON", result["dataType"]);
            Assert.Equal("metadata", result["additionalInfo"]);
        }

        /// <summary>
        /// Tests that the service throws an exception when parsing an invalid address.
        /// </summary>
        [Fact]
        public void ParseMemoryAddress_WithInvalidAddress_ThrowsException()
        {
            // Arrange
            var address = "invalid-address";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _memoryAddressService.ParseMemoryAddress(address));
            Assert.Contains("Invalid memory address format", exception.Message);
        }

        /// <summary>
        /// Tests that the service generates an input address for a processor correctly.
        /// </summary>
        [Fact]
        public void GenerateProcessorInputAddress_WithValidParameters_ReturnsCorrectAddress()
        {
            // Arrange
            var executionContext = new ExecutionContext
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                BranchId = "main",
                Parameters = new Dictionary<string, object>()
            };
            var processorId = "processor-001";

            // Act
            var result = _memoryAddressService.GenerateProcessorInputAddress(executionContext, processorId);

            // Assert
            Assert.Equal("exec-001:flow-001:PROCESS:main:processor-001:Input", result);
        }

        /// <summary>
        /// Tests that the service generates an output address for a processor correctly.
        /// </summary>
        [Fact]
        public void GenerateProcessorOutputAddress_WithValidParameters_ReturnsCorrectAddress()
        {
            // Arrange
            var executionContext = new ExecutionContext
            {
                ExecutionId = "exec-001",
                FlowId = "flow-001",
                BranchId = "main",
                Parameters = new Dictionary<string, object>()
            };
            var processorId = "processor-001";

            // Act
            var result = _memoryAddressService.GenerateProcessorOutputAddress(executionContext, processorId);

            // Assert
            Assert.Equal("exec-001:flow-001:PROCESS:main:processor-001:Output", result);
        }
    }
}
