using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Tests.Validation;

public class ValidationExtensionsTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("value", true)]
    public void ValidateNotNullOrEmpty_ReturnsExpectedResult(string? value, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = value.ValidateNotNullOrEmpty(paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null or empty", result.Message);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("\t", false)]
    [InlineData("value", true)]
    public void ValidateNotNullOrWhiteSpace_ReturnsExpectedResult(string? value, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = value.ValidateNotNullOrWhiteSpace(paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null, empty, or whitespace", result.Message);
        }
    }

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    public void ValidateRange_ReturnsExpectedResult(int value, int min, int max, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = value.ValidateRange(min, max, paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains($"must be between {min} and {max}", result.Message);
        }
    }

    [Theory]
    [InlineData("abc123", @"^[a-z0-9]+$", true)]
    [InlineData("ABC123", @"^[a-z0-9]+$", false)]
    [InlineData("123", @"^\d+$", true)]
    [InlineData("abc", @"^\d+$", false)]
    public void ValidatePattern_ReturnsExpectedResult(string value, string pattern, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = value.ValidatePattern(pattern, paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("does not match the required pattern", result.Message);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(new int[0], false)]
    [InlineData(new[] { 1 }, true)]
    public void ValidateNotNullOrEmpty_WithCollection_ReturnsExpectedResult(int[]? collection, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = collection.ValidateNotNullOrEmpty(paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null or empty", result.Message);
        }
    }

    [Fact]
    public void ThrowIfInvalid_WithValidResult_DoesNotThrow()
    {
        // Arrange
        var result = ValidationResult.Success();

        // Act & Assert
        result.ThrowIfInvalid(); // Should not throw
    }

    [Fact]
    public void ThrowIfInvalid_WithInvalidResult_ThrowsValidationException()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new[] { "Error 1", "Error 2" };
        var result = ValidationResult.Error(message, errors);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => result.ThrowIfInvalid());
        Assert.Equal(message, exception.Message);
        Assert.Equal(errors, exception.ValidationErrors);
    }
}
