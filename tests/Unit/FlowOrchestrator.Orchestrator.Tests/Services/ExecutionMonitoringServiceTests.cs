using ExecutionContext = FlowOrchestrator.Abstractions.Common.ExecutionContext;
using ExecutionStatus = FlowOrchestrator.Abstractions.Common.ExecutionStatus;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Repositories;
using FlowOrchestrator.Orchestrator.Services;
using FlowOrchestrator.Orchestrator.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FlowOrchestrator.Orchestrator.Tests.Services
{
    /// <summary>
    /// Tests for the ExecutionMonitoringService class.
    /// </summary>
    public class ExecutionMonitoringServiceTests
    {
        private readonly Mock<ILogger<ExecutionMonitoringService>> _loggerMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly TelemetryService _telemetryService;
        private readonly ExecutionMonitoringService _executionMonitoringService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionMonitoringServiceTests"/> class.
        /// </summary>
        public ExecutionMonitoringServiceTests()
        {
            _loggerMock = new Mock<ILogger<ExecutionMonitoringService>>();
            _executionRepositoryMock = new Mock<IExecutionRepository>();
            _messageBusMock = new Mock<IMessageBus>();
            _telemetryService = MockTelemetryServiceFactory.Create();

            _executionMonitoringService = new ExecutionMonitoringService(
                _loggerMock.Object,
                _executionRepositoryMock.Object,
                _messageBusMock.Object,
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
            parameters.SetParameter("MonitoringIntervalSeconds", "60");
            parameters.SetParameter("ExecutionTimeoutSeconds", "3600");

            // Act
            _executionMonitoringService.Initialize(parameters);

            // Assert
            Assert.Equal(FlowOrchestrator.Abstractions.Common.ServiceState.READY, _executionMonitoringService.GetState());
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
            var exception = Assert.Throws<ArgumentException>(() => _executionMonitoringService.Initialize(parameters));
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
            parameters.SetParameter("MonitoringIntervalSeconds", "60");
            parameters.SetParameter("ExecutionTimeoutSeconds", "3600");
            _executionMonitoringService.Initialize(parameters);

            // Act
            _executionMonitoringService.Terminate();

            // Assert
            Assert.Equal(FlowOrchestrator.Abstractions.Common.ServiceState.TERMINATED, _executionMonitoringService.GetState());
        }

        /// <summary>
        /// Tests that the service checks for timed out executions successfully.
        /// </summary>
        [Fact]
        public async Task CheckForTimedOutExecutionsAsync_WithTimedOutExecutions_UpdatesStatus()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MonitoringIntervalSeconds", "60");
            parameters.SetParameter("ExecutionTimeoutSeconds", "3600");
            _executionMonitoringService.Initialize(parameters);

            var timedOutExecutions = new List<ExecutionContext>
            {
                new ExecutionContext
                {
                    ExecutionId = "exec-001",
                    FlowId = "flow-001",
                    Status = ExecutionStatus.RUNNING,
                    StartTimestamp = DateTime.UtcNow.AddHours(-2) // 2 hours ago, exceeding the timeout
                },
                new ExecutionContext
                {
                    ExecutionId = "exec-002",
                    FlowId = "flow-002",
                    Status = ExecutionStatus.RUNNING,
                    StartTimestamp = DateTime.UtcNow.AddHours(-3) // 3 hours ago, exceeding the timeout
                }
            };

            _executionRepositoryMock.Setup(r => r.GetRunningExecutionsAsync())
                .ReturnsAsync(timedOutExecutions);
            _executionRepositoryMock.Setup(r => r.UpdateExecutionStatusAsync(It.IsAny<string>(), ExecutionStatus.TIMED_OUT))
                .ReturnsAsync(true);

            // Act
            await _executionMonitoringService.CheckForTimedOutExecutionsAsync();

            // Assert
            _executionRepositoryMock.Verify(r => r.UpdateExecutionStatusAsync("exec-001", ExecutionStatus.TIMED_OUT), Times.Once);
            _executionRepositoryMock.Verify(r => r.UpdateExecutionStatusAsync("exec-002", ExecutionStatus.TIMED_OUT), Times.Once);
        }

        /// <summary>
        /// Tests that the service doesn't update status for executions that haven't timed out.
        /// </summary>
        [Fact]
        public async Task CheckForTimedOutExecutionsAsync_WithActiveExecutions_DoesNotUpdateStatus()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MonitoringIntervalSeconds", "60");
            parameters.SetParameter("ExecutionTimeoutSeconds", "3600");
            _executionMonitoringService.Initialize(parameters);

            var activeExecutions = new List<ExecutionContext>
            {
                new ExecutionContext
                {
                    ExecutionId = "exec-001",
                    FlowId = "flow-001",
                    Status = ExecutionStatus.RUNNING,
                    StartTimestamp = DateTime.UtcNow.AddMinutes(-30) // 30 minutes ago, within the timeout
                },
                new ExecutionContext
                {
                    ExecutionId = "exec-002",
                    FlowId = "flow-002",
                    Status = ExecutionStatus.RUNNING,
                    StartTimestamp = DateTime.UtcNow.AddHours(-1) // 1 hour ago, within the timeout
                }
            };

            _executionRepositoryMock.Setup(r => r.GetRunningExecutionsAsync())
                .ReturnsAsync(activeExecutions);

            // Act
            await _executionMonitoringService.CheckForTimedOutExecutionsAsync();

            // Assert
            _executionRepositoryMock.Verify(r => r.UpdateExecutionStatusAsync(It.IsAny<string>(), It.IsAny<ExecutionStatus>()), Times.Never);
        }

        /// <summary>
        /// Tests that the service handles exceptions during execution monitoring.
        /// </summary>
        [Fact]
        public async Task CheckForTimedOutExecutionsAsync_WithException_LogsError()
        {
            // Arrange
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();
            parameters.SetParameter("MonitoringIntervalSeconds", "60");
            parameters.SetParameter("ExecutionTimeoutSeconds", "3600");
            _executionMonitoringService.Initialize(parameters);

            _executionRepositoryMock.Setup(r => r.GetRunningExecutionsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _executionMonitoringService.CheckForTimedOutExecutionsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking for timed out executions")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
