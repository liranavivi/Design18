using FlowOrchestrator.Common.Validation;

namespace FlowOrchestrator.Common.Tests.Validation;

public class ValidationHelperTests
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
        var result = ValidationHelper.ValidateNotNullOrEmpty(value, paramName);

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
        var result = ValidationHelper.ValidateNotNullOrWhiteSpace(value, paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null, empty, or whitespace", result.Message);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("value", true)]
    public void ValidateNotNull_ReturnsExpectedResult(object? value, bool expectedIsValid)
    {
        // Arrange
        var paramName = "testParam";

        // Act
        var result = ValidationHelper.ValidateNotNull(value, paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null", result.Message);
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
        var result = ValidationHelper.ValidateRange(value, min, max, paramName);

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
        var result = ValidationHelper.ValidatePattern(value, pattern, paramName);

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
        var result = ValidationHelper.ValidateNotNullOrEmpty(collection, paramName);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.Contains(paramName, result.Message);
            Assert.Contains("cannot be null or empty", result.Message);
        }
    }
}
