using FlowOrchestrator.Common.Exceptions;
using FlowOrchestrator.Common.Utilities;

namespace FlowOrchestrator.Common.Tests.Utilities;

public class GuardTests
{
    [Fact]
    public void AgainstNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;
        var paramName = "testParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(value, paramName));
        Assert.Equal(paramName, exception.ParamName);
    }

    [Fact]
    public void AgainstNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        var value = "test";
        var paramName = "testParam";

        // Act & Assert
        Guard.AgainstNull(value, paramName); // Should not throw
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AgainstNullOrEmpty_WithNullOrEmptyValue_ThrowsArgumentException(string? value)
    {
        // Arrange
        var paramName = "testParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty(value, paramName));
        Assert.Equal(paramName, exception.ParamName);
        Assert.Contains("Value cannot be null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmpty_WithNonEmptyValue_DoesNotThrow()
    {
        // Arrange
        var value = "test";
        var paramName = "testParam";

        // Act & Assert
        Guard.AgainstNullOrEmpty(value, paramName); // Should not throw
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void AgainstNullOrWhiteSpace_WithNullOrWhiteSpaceValue_ThrowsArgumentException(string? value)
    {
        // Arrange
        var paramName = "testParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrWhiteSpace(value, paramName));
        Assert.Equal(paramName, exception.ParamName);
        Assert.Contains("Value cannot be null, empty, or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_WithNonWhiteSpaceValue_DoesNotThrow()
    {
        // Arrange
        var value = "test";
        var paramName = "testParam";

        // Act & Assert
        Guard.AgainstNullOrWhiteSpace(value, paramName); // Should not throw
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    public void AgainstNullOrEmpty_WithNullOrEmptyCollection_ThrowsArgumentException(int[]? collection)
    {
        // Arrange
        var paramName = "testParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty(collection, paramName));
        Assert.Equal(paramName, exception.ParamName);
        Assert.Contains("Value cannot be null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };
        var paramName = "testParam";

        // Act & Assert
        Guard.AgainstNullOrEmpty(collection, paramName); // Should not throw
    }

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(11, 1, 10)]
    public void AgainstOutOfRange_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(int value, int min, int max)
    {
        // Arrange
        var paramName = "testParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstOutOfRange(value, paramName, min, max));
        Assert.Equal(paramName, exception.ParamName);
        Assert.Contains($"Value must be between {min} and {max}", exception.Message);
    }

    [Theory]
    [InlineData(1, 1, 10)]
    [InlineData(5, 1, 10)]
    [InlineData(10, 1, 10)]
    public void AgainstOutOfRange_WithInRangeValue_DoesNotThrow(int value, int min, int max)
    {
        // Arrange
        var paramName = "testParam";

        // Act & Assert
        Guard.AgainstOutOfRange(value, paramName, min, max); // Should not throw
    }

    [Fact]
    public void AgainstInvalid_WithFalseCondition_ThrowsArgumentException()
    {
        // Arrange
        var condition = false;
        var paramName = "testParam";
        var message = "Test error message";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.AgainstInvalid(condition, paramName, message));
        Assert.Equal(paramName, exception.ParamName);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void AgainstInvalid_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        var condition = true;
        var paramName = "testParam";
        var message = "Test error message";

        // Act & Assert
        Guard.AgainstInvalid(condition, paramName, message); // Should not throw
    }

    [Fact]
    public void AgainstInvalidOperation_WithFalseCondition_ThrowsInvalidOperationException()
    {
        // Arrange
        var condition = false;
        var message = "Test error message";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Guard.AgainstInvalidOperation(condition, message));
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void AgainstInvalidOperation_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        var condition = true;
        var message = "Test error message";

        // Act & Assert
        Guard.AgainstInvalidOperation(condition, message); // Should not throw
    }

    [Fact]
    public void AgainstInvalidConfiguration_WithFalseCondition_ThrowsConfigurationException()
    {
        // Arrange
        var condition = false;
        var message = "Test error message";

        // Act & Assert
        var exception = Assert.Throws<ConfigurationException>(() => Guard.AgainstInvalidConfiguration(condition, message));
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void AgainstInvalidConfiguration_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        var condition = true;
        var message = "Test error message";

        // Act & Assert
        Guard.AgainstInvalidConfiguration(condition, message); // Should not throw
    }
}
