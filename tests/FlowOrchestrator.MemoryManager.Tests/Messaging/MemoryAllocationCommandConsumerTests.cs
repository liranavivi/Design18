using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Messaging.Messages;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.MemoryManager.Messaging;
using FlowOrchestrator.MemoryManager.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowOrchestrator.MemoryManager.Tests.Messaging
{
    public class MemoryAllocationCommandConsumerTests
    {
        private readonly Mock<ILogger<MemoryAllocationCommandConsumer>> _loggerMock;
        private readonly Mock<IMemoryManagerService> _memoryManagerMock;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly MemoryAllocationCommandConsumer _consumer;

        public MemoryAllocationCommandConsumerTests()
        {
            _loggerMock = new Mock<ILogger<MemoryAllocationCommandConsumer>>();
            _memoryManagerMock = new Mock<IMemoryManagerService>();
            _messageBusMock = new Mock<IMessageBus>();
            _consumer = new MemoryAllocationCommandConsumer(
                _loggerMock.Object,
                _memoryManagerMock.Object,
                _messageBusMock.Object);
        }

        [Fact]
        public async Task Consume_ValidCommand_AllocatesMemory()
        {
            // Arrange
            var command = new MemoryAllocationCommand
            {
                CommandId = Guid.NewGuid(),
                ExecutionId = "execution1",
                FlowId = "flow1",
                StepId = "step1",
                Size = 1024,
                MemoryType = "Default",
                TimeToLiveSeconds = 3600
            };

            var context = new ConsumeContext<MemoryAllocationCommand>(
                command,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            var memoryAddress = "execution1:flow1:step1:guid";
            _memoryManagerMock.Setup(m => m.AllocateMemoryAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
                .ReturnsAsync(memoryAddress);

            // Act
            await _consumer.Consume(context);

            // Assert
            _memoryManagerMock.Verify(m => m.AllocateMemoryAsync(
                It.Is<int>(size => size == 1024),
                It.Is<string>(execId => execId == "execution1"),
                It.Is<string>(flowId => flowId == "flow1"),
                It.Is<string>(stepId => stepId == "step1"),
                It.Is<string>(memType => memType == "Default"),
                It.Is<TimeSpan?>(ttl => ttl.HasValue && ttl.Value.TotalSeconds == 3600)),
                Times.Once);

            _messageBusMock.Verify(m => m.PublishAsync(
                It.Is<MemoryAllocationResult>(r =>
                    r.CommandId == command.CommandId &&
                    r.Success &&
                    r.MemoryAddress == memoryAddress &&
                    r.ExecutionId == "execution1" &&
                    r.FlowId == "flow1" &&
                    r.StepId == "step1"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Consume_MemoryAllocationFails_PublishesFailureResult()
        {
            // Arrange
            var command = new MemoryAllocationCommand
            {
                CommandId = Guid.NewGuid(),
                ExecutionId = "execution1",
                FlowId = "flow1",
                StepId = "step1",
                Size = 1024
            };

            var context = new ConsumeContext<MemoryAllocationCommand>(
                command,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            var exception = new Exception("Memory allocation failed");
            _memoryManagerMock.Setup(m => m.AllocateMemoryAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
                .ThrowsAsync(exception);

            // Act
            await _consumer.Consume(context);

            // Assert
            _messageBusMock.Verify(m => m.PublishAsync(
                It.Is<MemoryAllocationResult>(r =>
                    r.CommandId == command.CommandId &&
                    !r.Success &&
                    r.ErrorMessage == exception.Message &&
                    r.ExecutionId == "execution1" &&
                    r.FlowId == "flow1" &&
                    r.StepId == "step1"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
