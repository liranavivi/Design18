using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Tests.Common
{
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
            Assert.True(parameters.Parameters.ContainsKey("key1"));
            Assert.Equal("value1", parameters.Parameters["key1"]);
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
            Assert.Equal("updated", parameters.Parameters["key1"]);
        }
    }
}
