using FlowOrchestrator.Common.Configuration;
using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Tests.Configuration;

public class ConfigurationBaseTests
{
    private class TestConfiguration : ConfigurationBase
    {
        public string TestProperty { get; set; } = string.Empty;

        public override ValidationResult Validate()
        {
            if (string.IsNullOrEmpty(TestProperty))
            {
                return ValidationResult.Error("TestProperty cannot be empty.");
            }

            return ValidationResult.Success();
        }
    }

    private class SerializableTestConfiguration : ConfigurationBase, ISerializableConfiguration
    {
        public string TestProperty { get; set; } = string.Empty;

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public string Serialize()
        {
            return $"{Name}|{Description}|{Version}|{TestProperty}";
        }

        public bool Deserialize(string serializedConfiguration)
        {
            if (string.IsNullOrEmpty(serializedConfiguration))
            {
                return false;
            }

            var parts = serializedConfiguration.Split('|');
            if (parts.Length != 4)
            {
                return false;
            }

            Name = parts[0];
            Description = parts[1];
            Version = parts[2];
            TestProperty = parts[3];

            return true;
        }
    }

    [Fact]
    public void ConfigurationBase_DefaultProperties_HaveExpectedValues()
    {
        // Arrange & Act
        var configuration = new TestConfiguration();

        // Assert
        Assert.Equal(string.Empty, configuration.Name);
        Assert.Equal(string.Empty, configuration.Description);
        Assert.Equal("1.0.0", configuration.Version);
    }

    [Fact]
    public void ConfigurationBase_SetProperties_RetainsValues()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "Test Configuration",
            Description = "A test configuration",
            Version = "2.0.0",
            TestProperty = "Test Value"
        };

        // Act & Assert
        Assert.Equal("Test Configuration", configuration.Name);
        Assert.Equal("A test configuration", configuration.Description);
        Assert.Equal("2.0.0", configuration.Version);
        Assert.Equal("Test Value", configuration.TestProperty);
    }

    [Fact]
    public void Validate_WithValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            TestProperty = "Test Value"
        };

        // Act
        var result = configuration.Validate();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidConfiguration_ReturnsError()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            TestProperty = string.Empty
        };

        // Act
        var result = configuration.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("TestProperty cannot be empty", result.Message);
    }

    [Fact]
    public void ISerializableConfiguration_Serialize_ReturnsExpectedString()
    {
        // Arrange
        var configuration = new SerializableTestConfiguration
        {
            Name = "Test Configuration",
            Description = "A test configuration",
            Version = "2.0.0",
            TestProperty = "Test Value"
        };

        // Act
        var serialized = configuration.Serialize();

        // Assert
        Assert.Equal("Test Configuration|A test configuration|2.0.0|Test Value", serialized);
    }

    [Fact]
    public void ISerializableConfiguration_Deserialize_WithValidString_ReturnsTrue()
    {
        // Arrange
        var configuration = new SerializableTestConfiguration();
        var serialized = "Test Configuration|A test configuration|2.0.0|Test Value";

        // Act
        var result = configuration.Deserialize(serialized);

        // Assert
        Assert.True(result);
        Assert.Equal("Test Configuration", configuration.Name);
        Assert.Equal("A test configuration", configuration.Description);
        Assert.Equal("2.0.0", configuration.Version);
        Assert.Equal("Test Value", configuration.TestProperty);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("Invalid", false)]
    public void ISerializableConfiguration_Deserialize_WithInvalidString_ReturnsFalse(string? serialized, bool expected)
    {
        // Arrange
        var configuration = new SerializableTestConfiguration();

        // Act
        var result = configuration.Deserialize(serialized!);

        // Assert
        Assert.Equal(expected, result);
    }
}
