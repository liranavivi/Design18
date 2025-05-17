using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using ConfigurationParameters = FlowOrchestrator.Abstractions.Common.ConfigurationParameters;
using BranchExecutionContext = FlowOrchestrator.Orchestrator.BranchExecutionContext;
using ServiceState = FlowOrchestrator.Abstractions.Common.ServiceState;
using System.Collections.Generic;
using FlowOrchestrator.Abstractions.Entities;
using FlowOrchestrator.Abstractions.Services;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using FlowOrchestrator.Orchestrator.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlowOrchestrator.Orchestrator.Tests.Services
{
    /// <summary>
    /// Tests for the OrchestratorService class.
    /// </summary>
    public class OrchestratorServiceTests
    {
        private readonly Mock<ILogger<OrchestratorService>> _loggerMock;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly TelemetryService _telemetryService;
        private readonly OrchestratorService _orchestratorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorServiceTests"/> class.
        /// </summary>
        public OrchestratorServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrchestratorService>>();
            _messageBusMock = new Mock<IMessageBus>();
            _executionRepositoryMock = new Mock<IExecutionRepository>();
            _errorHandlingService = MockErrorHandlingServiceFactory.Create();
            _telemetryService = MockTelemetryServiceFactory.Create();

            _orchestratorService = new OrchestratorService(
                _loggerMock.Object,
                _messageBusMock.Object,
                _executionRepositoryMock.Object,
                _errorHandlingService,
                _telemetryService);
        }

        /// <summary>
        /// Tests that the service initializes successfully with valid configuration parameters.
        /// </summary>
        [Fact]
        public void Initialize_WithValidParameters_Succeeds()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MaxConcurrentExecutions", "10");
            parameters.SetParameter("ExecutionTimeoutSeconds", "300");

            // Act
            _orchestratorService.Initialize(parameters);

            // Assert
            Assert.Equal(ServiceState.READY, _orchestratorService.GetState());
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("initialized successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the service throws an exception when initialized with invalid parameters.
        /// </summary>
        [Fact]
        public void Initialize_WithInvalidParameters_ThrowsException()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            // Missing required parameters

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _orchestratorService.Initialize(parameters));
            Assert.Contains("Invalid configuration", exception.Message);
        }

        /// <summary>
        /// Tests that the service terminates successfully.
        /// </summary>
        [Fact]
        public void Terminate_Succeeds()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MaxConcurrentExecutions", "10");
            parameters.SetParameter("ExecutionTimeoutSeconds", "300");
            _orchestratorService.Initialize(parameters);

            // Act
            _orchestratorService.Terminate();

            // Assert
            Assert.Equal(ServiceState.TERMINATED, _orchestratorService.GetState());
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("terminated successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the service starts a flow execution successfully.
        /// </summary>
        [Fact]
        public async Task StartFlowExecutionAsync_WithValidFlow_ReturnsExecutionContext()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MaxConcurrentExecutions", "10");
            parameters.SetParameter("ExecutionTimeoutSeconds", "300");
            _orchestratorService.Initialize(parameters);

            var flowEntity = new Mock<IFlowEntity>();
            flowEntity.Setup(f => f.FlowId).Returns("flow-001");
            flowEntity.Setup(f => f.Name).Returns("Test Flow");
            flowEntity.Setup(f => f.ImporterServiceId).Returns("importer-001");
            flowEntity.Setup(f => f.ProcessorServiceIds).Returns(new List<string> { "processor-001" });
            flowEntity.Setup(f => f.ExporterServiceIds).Returns(new List<string> { "exporter-001" });

            var executionParams = new Dictionary<string, object>
            {
                { "SourcePath", "C:\\Data\\Input" },
                { "DestinationPath", "C:\\Data\\Output" }
            };

            _executionRepositoryMock.Setup(r => r.SaveExecutionContextAsync(It.IsAny<ExecutionContext>()))
                .Returns(Task.CompletedTask);
            _executionRepositoryMock.Setup(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orchestratorService.StartFlowExecutionAsync(flowEntity.Object, executionParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("flow-001", result.FlowId);
            Assert.Equal(ExecutionStatus.RUNNING, result.Status);
            Assert.NotEmpty(result.ExecutionId);
            Assert.Equal(executionParams, result.Parameters);

            _executionRepositoryMock.Verify(r => r.SaveExecutionContextAsync(It.IsAny<ExecutionContext>()), Times.Once);
            _executionRepositoryMock.Verify(r => r.SaveBranchContextAsync(It.IsAny<BranchExecutionContext>()), Times.Once);
        }

        /// <summary>
        /// Tests that the service handles exceptions during flow execution start.
        /// </summary>
        [Fact]
        public async Task StartFlowExecutionAsync_WithException_HandlesError()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MaxConcurrentExecutions", "10");
            parameters.SetParameter("ExecutionTimeoutSeconds", "300");
            _orchestratorService.Initialize(parameters);

            var flowEntity = new Mock<IFlowEntity>();
            flowEntity.Setup(f => f.FlowId).Returns("flow-001");
            flowEntity.Setup(f => f.Name).Returns("Test Flow");
            flowEntity.Setup(f => f.ImporterServiceId).Returns("importer-001");
            flowEntity.Setup(f => f.ProcessorServiceIds).Returns(new List<string> { "processor-001" });
            flowEntity.Setup(f => f.ExporterServiceIds).Returns(new List<string> { "exporter-001" });

            var executionParams = new Dictionary<string, object>
            {
                { "SourcePath", "C:\\Data\\Input" },
                { "DestinationPath", "C:\\Data\\Output" }
            };

            _executionRepositoryMock.Setup(r => r.SaveExecutionContextAsync(It.IsAny<ExecutionContext>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _orchestratorService.StartFlowExecutionAsync(flowEntity.Object, executionParams));

            Assert.Equal("Test exception", exception.Message);
            // We can't verify the call to HandleError since we're using a concrete implementation
        }
    }
}
