using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Orchestrator.Models;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlowOrchestrator.Orchestrator.Tests.Mocks
{
    /// <summary>
    /// Factory for creating a mock ErrorHandlingService.
    /// </summary>
    public static class MockErrorHandlingServiceFactory
    {
        /// <summary>
        /// Creates a mock ErrorHandlingService.
        /// </summary>
        /// <returns>A mock ErrorHandlingService.</returns>
        public static ErrorHandlingService Create()
        {
            var mock = new Mock<ErrorHandlingService>(
                new Mock<ILogger<ErrorHandlingService>>().Object,
                new Mock<IMessageBus>().Object);

            mock.Setup(m => m.HandleError(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ExecutionError>(),
                    It.IsAny<FlowOrchestrator.Abstractions.Common.ExecutionContext>()))
                .Returns(RecoveryAction.RETRY);

            return mock.Object;
        }
    }
}
