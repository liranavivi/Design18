using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Tests.Validation;

public class ValidationResultTests
{
    [Fact]
    public void Success_CreatesValidResult()
    {
        // Arrange & Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Validation successful.", result.Message);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Success_WithCustomMessage_CreatesValidResult()
    {
        // Arrange
        var message = "Custom success message";

        // Act
        var result = ValidationResult.Success(message);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(message, result.Message);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Warning_CreatesValidResultWithWarnings()
    {
        // Arrange
        var message = "Warning message";
        var warnings = new[] { "Warning 1", "Warning 2" };

        // Act
        var result = ValidationResult.Warning(message, warnings);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(message, result.Message);
        Assert.Empty(result.Errors);
        Assert.Equal(warnings, result.Warnings);
    }

    [Fact]
    public void Error_CreatesInvalidResultWithErrors()
    {
        // Arrange
        var message = "Error message";
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = ValidationResult.Error(message, errors);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(message, result.Message);
        Assert.Equal(errors, result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Error_WithoutErrors_CreatesInvalidResultWithMessageAsError()
    {
        // Arrange
        var message = "Error message";

        // Act
        var result = ValidationResult.Error(message);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(message, result.Message);
        Assert.Single(result.Errors);
        Assert.Equal(message, result.Errors[0]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Combine_WithEmptyList_ReturnsSuccessResult()
    {
        // Arrange
        var results = new List<ValidationResult>();

        // Act
        var result = ValidationResult.Combine(results);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Combined validation successful.", result.Message);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Combine_WithAllValidResults_ReturnsValidResult()
    {
        // Arrange
        var results = new List<ValidationResult>
        {
            ValidationResult.Success("Success 1"),
            ValidationResult.Success("Success 2"),
            ValidationResult.Warning("Warning", "Warning 1")
        };

        // Act
        var result = ValidationResult.Combine(results);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Combined validation successful.", result.Message);
        Assert.Empty(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Equal("Warning 1", result.Warnings[0]);
    }

    [Fact]
    public void Combine_WithSomeInvalidResults_ReturnsInvalidResult()
    {
        // Arrange
        var results = new List<ValidationResult>
        {
            ValidationResult.Success("Success 1"),
            ValidationResult.Error("Error 1", "Error detail 1"),
            ValidationResult.Warning("Warning", "Warning 1")
        };

        // Act
        var result = ValidationResult.Combine(results);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Combined validation failed.", result.Message);
        Assert.Single(result.Errors);
        Assert.Equal("Error detail 1", result.Errors[0]);
        Assert.Single(result.Warnings);
        Assert.Equal("Warning 1", result.Warnings[0]);
    }
}
