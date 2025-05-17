using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Domain.Entities;
using static FlowOrchestrator.Domain.Entities.AbstractProcessingChainEntity;

namespace FlowOrchestrator.Domain.Tests.Entities
{
    /// <summary>
    /// Tests for the ProcessingChainEntity class.
    /// </summary>
    public class ProcessingChainEntityTests
    {
        [Fact]
        public void ProcessingChainEntity_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var processingChainEntity = new ProcessingChainEntity();

            // Assert
            Assert.Equal(string.Empty, processingChainEntity.ServiceId);
            Assert.Equal("PROCESSING_CHAIN", processingChainEntity.ServiceType);
            Assert.Equal("1.0.0", processingChainEntity.Version);
            Assert.Equal(string.Empty, processingChainEntity.ChainId);
            Assert.Equal(string.Empty, processingChainEntity.Name);
            Assert.Equal(string.Empty, processingChainEntity.Description);
            Assert.Empty(processingChainEntity.ProcessorServiceIds);
            // ProcessingChainEntity implements IService, so it has GetState() method
            Assert.Equal(ServiceState.READY, processingChainEntity.GetState());
            Assert.False(processingChainEntity.IsEnabled);
        }

        [Fact]
        public void ProcessingChainEntity_ParameterizedConstructor_SetsProvidedValues()
        {
            // Arrange & Act
            var processingChainEntity = new ProcessingChainEntity("chain-001", "Test Chain");

            // Assert
            Assert.Equal("chain-001", processingChainEntity.ChainId);
            Assert.Equal("Test Chain", processingChainEntity.Name);
        }

        [Fact]
        public void ProcessingChainEntity_SetProperties_RetainsValues()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                ServiceId = "service-001",
                ChainId = "chain-001",
                Name = "Test Chain",
                Description = "A test processing chain",
                ProcessorServiceIds = new List<string> { "processor-001", "processor-002" },
                // State is managed internally
                IsEnabled = true
            };

            // Act & Assert
            Assert.Equal("service-001", processingChainEntity.ServiceId);
            Assert.Equal("chain-001", processingChainEntity.ChainId);
            Assert.Equal("Test Chain", processingChainEntity.Name);
            Assert.Equal("A test processing chain", processingChainEntity.Description);
            Assert.Equal(2, processingChainEntity.ProcessorServiceIds.Count);
            Assert.Contains("processor-001", processingChainEntity.ProcessorServiceIds);
            Assert.Contains("processor-002", processingChainEntity.ProcessorServiceIds);
            Assert.Equal(ServiceState.READY, processingChainEntity.GetState());
            Assert.True(processingChainEntity.IsEnabled);
        }

        [Fact]
        public void GetEntityId_ReturnsChainId()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity { ChainId = "chain-001" };

            // Act
            var entityId = processingChainEntity.GetEntityId();

            // Assert
            Assert.Equal("chain-001", entityId);
        }

        [Fact]
        public void GetEntityType_ReturnsProcessingChainEntity()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity();

            // Act
            var entityType = processingChainEntity.GetEntityType();

            // Assert
            Assert.Equal("ProcessingChainEntity", entityType);
        }

        [Fact]
        public void Validate_WithValidProcessingChain_ReturnsSuccess()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                ChainId = "chain-001",
                Name = "Test Chain",
                ProcessorServiceIds = new List<string> { "processor-001" },
                Structure = new ProcessingChainStructure
                {
                    Nodes = new List<ProcessorNode>
                    {
                        new ProcessorNode { NodeId = "node-001", ProcessorId = "processor-001" }
                    },
                    Connections = new List<ProcessorConnection>()
                }
            };

            // Act
            var result = processingChainEntity.Validate();

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithMissingChainId_ReturnsError()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                Name = "Test Chain",
                ProcessorServiceIds = new List<string> { "processor-001" }
            };

            // Act
            var result = processingChainEntity.Validate();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "CHAIN_ID_REQUIRED");
        }

        [Fact]
        public void Validate_WithMissingName_ReturnsError()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                ChainId = "chain-001",
                ProcessorServiceIds = new List<string> { "processor-001" }
            };

            // Act
            var result = processingChainEntity.Validate();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "CHAIN_NAME_REQUIRED");
        }

        [Fact]
        public void Validate_WithMissingProcessorServiceIds_ReturnsError()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                ChainId = "chain-001",
                Name = "Test Chain"
            };

            // Act
            var result = processingChainEntity.Validate();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "PROCESSOR_SERVICE_IDS_REQUIRED");
        }

        [Fact]
        public void Validate_WithEmptyProcessorServiceIds_ReturnsError()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity
            {
                ChainId = "chain-001",
                Name = "Test Chain",
                ProcessorServiceIds = new List<string>()
            };

            // Act
            var result = processingChainEntity.Validate();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "PROCESSOR_SERVICE_IDS_REQUIRED");
        }

        [Fact]
        public void GetState_ReturnsReadyState()
        {
            // Arrange
            var processingChainEntity = new ProcessingChainEntity();

            // Act
            var state = processingChainEntity.GetState();

            // Assert
            Assert.Equal(ServiceState.READY, state);
        }
    }
}
