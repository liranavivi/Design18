using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Configuration;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowOrchestrator.Infrastructure.Tests.Messaging
{
    public class MassTransitIntegrationTests
    {
        [Fact]
        public async Task PublishMessage_ConsumerRegistered_MessageIsConsumed()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add test logger
            services.AddLogging(builder => builder.AddConsole());
            
            // Configure MassTransit with in-memory transport
            services.AddFlowOrchestratorMessageBus(options =>
            {
                options.TransportType = TransportType.InMemory;
            });
            
            // Create a test consumer
            var testConsumer = new TestConsumer();
            services.AddSingleton<IMessageConsumer<TestMessage>>(testConsumer);
            
            // Register the consumer
            services.AddFlowOrchestratorConsumer<TestMessage, TestConsumer>();
            
            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();
            
            // Get the message bus
            var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
            
            // Create a test message
            var testMessage = new TestMessage { Content = "Test" };
            
            // Act
            await messageBus.PublishAsync(testMessage);
            
            // Wait for the message to be consumed
            await Task.Delay(1000);
            
            // Assert
            Assert.True(testConsumer.MessageReceived);
            Assert.Equal("Test", testConsumer.ReceivedMessage?.Content);
        }
        
        [Fact]
        public async Task SendMessage_ConsumerRegistered_MessageIsConsumed()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add test logger
            services.AddLogging(builder => builder.AddConsole());
            
            // Configure MassTransit with in-memory transport
            services.AddFlowOrchestratorMessageBus(options =>
            {
                options.TransportType = TransportType.InMemory;
            });
            
            // Create a test consumer
            var testConsumer = new TestConsumer();
            services.AddSingleton<IMessageConsumer<TestMessage>>(testConsumer);
            
            // Register the consumer
            services.AddFlowOrchestratorConsumer<TestMessage, TestConsumer>();
            
            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();
            
            // Get the message bus
            var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
            
            // Create a test message
            var testMessage = new TestMessage { Content = "Test" };
            
            // Act
            await messageBus.SendAsync(testMessage, "test-queue");
            
            // Wait for the message to be consumed
            await Task.Delay(1000);
            
            // Assert
            Assert.True(testConsumer.MessageReceived);
            Assert.Equal("Test", testConsumer.ReceivedMessage?.Content);
        }
        
        private class TestMessage
        {
            public string Content { get; set; }
        }
        
        private class TestConsumer : IMessageConsumer<TestMessage>
        {
            public bool MessageReceived { get; private set; }
            public TestMessage ReceivedMessage { get; private set; }
            
            public Task Consume(ConsumeContext<TestMessage> context)
            {
                MessageReceived = true;
                ReceivedMessage = context.Message;
                return Task.CompletedTask;
            }
        }
    }
}
