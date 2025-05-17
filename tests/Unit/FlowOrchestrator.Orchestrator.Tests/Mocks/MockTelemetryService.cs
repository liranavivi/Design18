using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Orchestrator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace FlowOrchestrator.Orchestrator.Tests.Mocks
{
    /// <summary>
    /// Factory for creating a mock TelemetryService.
    /// </summary>
    public static class MockTelemetryServiceFactory
    {
        /// <summary>
        /// Creates a mock TelemetryService.
        /// </summary>
        /// <returns>A mock TelemetryService.</returns>
        public static TelemetryService Create()
        {
            var mock = new Mock<TelemetryService>(new Mock<ILogger<TelemetryService>>().Object);

            mock.Setup(m => m.RecordFlowExecutionStart(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((Activity?)null);

            mock.Setup(m => m.RecordBranchExecutionStart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Activity>()))
                .Returns((Activity?)null);

            return mock.Object;
        }
    }
}
