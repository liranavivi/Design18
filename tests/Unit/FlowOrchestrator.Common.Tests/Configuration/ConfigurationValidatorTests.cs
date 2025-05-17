using FlowOrchestrator.Common.Configuration;
using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Tests.Configuration;

public class ConfigurationValidatorTests
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

    [Fact]
    public void Validate_WithNullConfiguration_ReturnsError()
    {
        // Arrange
        TestConfiguration? configuration = null;

        // Act
        var result = ConfigurationValidator.Validate(configuration!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configuration cannot be null.", result.Message);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = string.Empty,
            TestProperty = "Test Value"
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configuration name cannot be empty.", result.Message);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "   ",
            TestProperty = "Test Value"
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configuration name cannot be empty.", result.Message);
    }

    [Fact]
    public void Validate_WithEmptyVersion_ReturnsError()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "Test Configuration",
            Version = string.Empty,
            TestProperty = "Test Value"
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configuration version cannot be empty.", result.Message);
    }

    [Fact]
    public void Validate_WithWhitespaceVersion_ReturnsError()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "Test Configuration",
            Version = "   ",
            TestProperty = "Test Value"
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configuration version cannot be empty.", result.Message);
    }

    [Fact]
    public void Validate_WithValidConfiguration_CallsConfigurationValidate()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "Test Configuration",
            Version = "1.0.0",
            TestProperty = string.Empty // This will make the configuration's Validate method return an error
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("TestProperty cannot be empty.", result.Message);
    }

    [Fact]
    public void Validate_WithFullyValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var configuration = new TestConfiguration
        {
            Name = "Test Configuration",
            Version = "1.0.0",
            TestProperty = "Test Value"
        };

        // Act
        var result = ConfigurationValidator.Validate(configuration);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateAll_WithNullCollection_ReturnsError()
    {
        // Arrange
        IEnumerable<ConfigurationBase>? configurations = null;

        // Act
        var result = ConfigurationValidator.ValidateAll(configurations!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Configurations collection cannot be null.", result.Message);
    }

    [Fact]
    public void ValidateAll_WithEmptyCollection_ReturnsSuccess()
    {
        // Arrange
        var configurations = new List<ConfigurationBase>();

        // Act
        var result = ConfigurationValidator.ValidateAll(configurations);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Combined validation successful.", result.Message);
    }

    [Fact]
    public void ValidateAll_WithAllValidConfigurations_ReturnsSuccess()
    {
        // Arrange
        var configurations = new List<ConfigurationBase>
        {
            new TestConfiguration
            {
                Name = "Config 1",
                Version = "1.0.0",
                TestProperty = "Value 1"
            },
            new TestConfiguration
            {
                Name = "Config 2",
                Version = "1.0.0",
                TestProperty = "Value 2"
            }
        };

        // Act
        var result = ConfigurationValidator.ValidateAll(configurations);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Combined validation successful.", result.Message);
    }

    [Fact]
    public void ValidateAll_WithSomeInvalidConfigurations_ReturnsError()
    {
        // Arrange
        var configurations = new List<ConfigurationBase>
        {
            new TestConfiguration
            {
                Name = "Config 1",
                Version = "1.0.0",
                TestProperty = "Value 1"
            },
            new TestConfiguration
            {
                Name = "Config 2",
                Version = "1.0.0",
                TestProperty = string.Empty // This will make the configuration's Validate method return an error
            }
        };

        // Act
        var result = ConfigurationValidator.ValidateAll(configurations);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Combined validation failed.", result.Message);
        Assert.Contains("TestProperty cannot be empty.", result.Errors);
    }
}
