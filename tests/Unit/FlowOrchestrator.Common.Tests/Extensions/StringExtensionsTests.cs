using FlowOrchestrator.Common.Extensions;
using System.Text;

namespace FlowOrchestrator.Common.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", false)]
    [InlineData("test", false)]
    public void IsNullOrEmpty_ReturnsExpectedResult(string? value, bool expected)
    {
        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("\t", true)]
    [InlineData("\n", true)]
    [InlineData("test", false)]
    public void IsNullOrWhiteSpace_ReturnsExpectedResult(string? value, bool expected)
    {
        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Truncate_WithShortString_ReturnsOriginalString()
    {
        // Arrange
        var value = "Short string";
        var maxLength = 20;

        // Act
        var result = value.Truncate(maxLength);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Truncate_WithLongString_ReturnsTruncatedString()
    {
        // Arrange
        var value = "This is a very long string that needs to be truncated";
        var maxLength = 20;

        // Act
        var result = value.Truncate(maxLength);

        // Assert
        Assert.Equal(maxLength, result.Length);
        Assert.Equal("This is a very lo...", result);
    }

    [Fact]
    public void Truncate_WithCustomSuffix_ReturnsTruncatedStringWithCustomSuffix()
    {
        // Arrange
        var value = "This is a very long string that needs to be truncated";
        var maxLength = 20;
        var suffix = " [more]";

        // Act
        var result = value.Truncate(maxLength, suffix);

        // Assert
        Assert.Equal(maxLength, result.Length);
        Assert.Equal("This is a ver [more]", result);
    }

    [Fact]
    public void ToByteArray_ReturnsCorrectByteArray()
    {
        // Arrange
        var value = "Test string";
        var expected = Encoding.UTF8.GetBytes(value);

        // Act
        var result = value.ToByteArray();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToByteArray_WithEncoding_ReturnsCorrectByteArray()
    {
        // Arrange
        var value = "Test string";
        var encoding = Encoding.ASCII;
        var expected = encoding.GetBytes(value);

        // Act
        var result = value.ToByteArray(encoding);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("HELLO WORLD", "HELLO WORLD")]
    [InlineData("Hello World", "Hello World")]
    [InlineData("", "")]
    [InlineData("a", "A")]
    public void ToTitleCase_ReturnsExpectedResult(string value, string expected)
    {
        // Act
        var result = value.ToTitleCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("helloWorld", "helloWorld")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    public void ToCamelCase_ReturnsExpectedResult(string value, string expected)
    {
        // Act
        var result = value.ToCamelCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello World", "HelloWorld")]
    [InlineData("  Spaces   Everywhere  ", "SpacesEverywhere")]
    [InlineData("Tabs\tand\tnewlines\n", "Tabsandnewlines")]
    [InlineData("", "")]
    public void RemoveWhitespace_ReturnsExpectedResult(string value, string expected)
    {
        // Act
        var result = value.RemoveWhitespace();

        // Assert
        Assert.Equal(expected, result);
    }
}
