﻿using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Services;
using Moq;

namespace FlowOrchestrator.Abstractions.Tests
{
    /// <summary>
    /// Tests for the IService interface.
    /// </summary>
    public class IServiceTests
    {
        private class TestService : IService
        {
            public string ServiceId { get; } = "TEST-SERVICE";
            public string Version { get; } = "1.0.0";
            public string ServiceType { get; } = "TEST";

            public ServiceState GetState()
            {
                return ServiceState.READY;
            }

            public void Initialize(ConfigurationParameters parameters)
            {
                // Test implementation
            }

            public void Terminate()
            {
                // Test implementation
            }
        }

        [Fact]
        public void IService_Properties_ShouldBeAccessible()
        {
            // Arrange
            var service = new TestService();

            // Act & Assert
            Assert.Equal("TEST-SERVICE", service.ServiceId);
            Assert.Equal("1.0.0", service.Version);
            Assert.Equal("TEST", service.ServiceType);
        }

        [Fact]
        public void IService_GetState_ShouldReturnServiceState()
        {
            // Arrange
            var service = new TestService();

            // Act
            var state = service.GetState();

            // Assert
            Assert.Equal(ServiceState.READY, state);
        }

        [Fact]
        public void IService_Initialize_ShouldAcceptConfigurationParameters()
        {
            // Arrange
            var service = new TestService();
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("TestKey", "TestValue");

            // Act & Assert (no exception means success)
            service.Initialize(parameters);
        }

        [Fact]
        public void IService_Terminate_ShouldBeCallable()
        {
            // Arrange
            var service = new TestService();

            // Act & Assert (no exception means success)
            service.Terminate();
        }
    }

    /// <summary>
    /// Tests for the ConfigurationParameters class.
    /// </summary>
    public class ConfigurationParametersTests
    {
        [Fact]
        public void GetParameter_ShouldReturnValue_WhenKeyExists()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act
            var value = parameters.GetParameter<string>("key1");

            // Assert
            Assert.Equal("value1", value);
        }

        [Fact]
        public void GetParameter_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
        {
            // Arrange
            var parameters = new ConfigurationParameters();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => parameters.GetParameter<string>("nonexistent"));
        }

        [Fact]
        public void GetParameter_ShouldThrowInvalidCastException_WhenTypeIsIncorrect()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => parameters.GetParameter<int>("key1"));
        }

        [Fact]
        public void TryGetParameter_ShouldReturnTrue_WhenKeyExistsAndTypeIsCorrect()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act
            var success = parameters.TryGetParameter<string>("key1", out var value);

            // Assert
            Assert.True(success);
            Assert.Equal("value1", value);
        }

        [Fact]
        public void TryGetParameter_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var parameters = new ConfigurationParameters();

            // Act
            var success = parameters.TryGetParameter<string>("nonexistent", out var value);

            // Assert
            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void TryGetParameter_ShouldReturnFalse_WhenTypeIsIncorrect()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act
            var success = parameters.TryGetParameter<int>("key1", out var value);

            // Assert
            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void GetParameterOrDefault_ShouldReturnValue_WhenKeyExists()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act
            var value = parameters.GetParameterOrDefault("key1", "default");

            // Assert
            Assert.Equal("value1", value);
        }

        [Fact]
        public void GetParameterOrDefault_ShouldReturnDefaultValue_WhenKeyDoesNotExist()
        {
            // Arrange
            var parameters = new ConfigurationParameters();

            // Act
            var value = parameters.GetParameterOrDefault("nonexistent", "default");

            // Assert
            Assert.Equal("default", value);
        }

        [Fact]
        public void SetParameter_ShouldAddNewParameter_WhenKeyDoesNotExist()
        {
            // Arrange
            var parameters = new ConfigurationParameters();

            // Act
            parameters.SetParameter("key1", "value1");

            // Assert
            Assert.True(parameters.ContainsKey("key1"));
            Assert.Equal("value1", parameters.GetParameter<string>("key1"));
        }

        [Fact]
        public void SetParameter_ShouldUpdateParameter_WhenKeyExists()
        {
            // Arrange
            var parameters = new ConfigurationParameters();
            parameters.SetParameter("key1", "value1");

            // Act
            parameters.SetParameter("key1", "updated");

            // Assert
            Assert.Equal("updated", parameters.GetParameter<string>("key1"));
        }
    }
}
