using FlowOrchestrator.Common.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FlowOrchestrator.Common.Tests.Extensions;

public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(Nullable<>), true)]
    public void IsNullable_ReturnsExpectedResult(Type type, bool expected)
    {
        // Act
        var result = type.IsNullable();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(int?), typeof(int))]
    [InlineData(typeof(bool?), typeof(bool))]
    [InlineData(typeof(string), typeof(string))]
    [InlineData(typeof(int), typeof(int))]
    public void GetUnderlyingType_ReturnsExpectedType(Type type, Type expected)
    {
        // Act
        var result = type.GetUnderlyingType();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(List<int>), typeof(IEnumerable<>), true)]
    [InlineData(typeof(List<int>), typeof(ICollection<>), true)]
    [InlineData(typeof(List<int>), typeof(IList<>), true)]
    [InlineData(typeof(Dictionary<string, int>), typeof(IEnumerable<>), true)]
    [InlineData(typeof(string), typeof(IEnumerable<>), true)]
    [InlineData(typeof(int), typeof(IComparable<>), true)]
    public void ImplementsInterface_ReturnsExpectedResult(Type type, Type interfaceType, bool expected)
    {
        // Act
        var result = type.ImplementsInterface(interfaceType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(List<int>), typeof(IList<int>), false)]
    [InlineData(typeof(List<int>), typeof(object), true)]
    [InlineData(typeof(string), typeof(object), true)]
    [InlineData(typeof(string), typeof(ValueType), false)]
    [InlineData(typeof(int), typeof(ValueType), true)]
    public void IsSubclassOf_ReturnsExpectedResult(Type type, Type baseType, bool expected)
    {
        // Act
        var result = type.IsSubclassOf(baseType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string), typeof(List<>), false)]
    [InlineData(typeof(int), typeof(List<>), false)]
    [InlineData(typeof(List<int>), typeof(string), false)]
    public void IsSubclassOfGeneric_ReturnsExpectedResult(Type? type, Type? genericType, bool expected)
    {
        // Act
        var result = type?.IsSubclassOfGeneric(genericType!) ?? false;

        // Assert
        Assert.Equal(expected, result);
    }

    private class TestClass
    {
        [Required]
        public string RequiredProperty { get; set; } = string.Empty;

        public string NormalProperty { get; set; } = string.Empty;

        [Required]
        public int RequiredIntProperty { get; set; }

        [ExcludeFromCodeCoverage]
        public void ExcludedMethod() { }

        public void NormalMethod() { }
    }

    [Fact]
    public void GetPropertiesWithAttribute_ReturnsPropertiesWithSpecifiedAttribute()
    {
        // Arrange
        var type = typeof(TestClass);

        // Act
        var properties = type.GetPropertiesWithAttribute<RequiredAttribute>().ToList();

        // Assert
        Assert.Equal(2, properties.Count);
        Assert.Contains(properties, p => p.Name == "RequiredProperty");
        Assert.Contains(properties, p => p.Name == "RequiredIntProperty");
        Assert.DoesNotContain(properties, p => p.Name == "NormalProperty");
    }

    [Fact]
    public void GetMethodsWithAttribute_ReturnsMethodsWithSpecifiedAttribute()
    {
        // Arrange
        var type = typeof(TestClass);

        // Act
        var methods = type.GetMethodsWithAttribute<ExcludeFromCodeCoverageAttribute>().ToList();

        // Assert
        Assert.Single(methods);
        Assert.Contains(methods, m => m.Name == "ExcludedMethod");
        Assert.DoesNotContain(methods, m => m.Name == "NormalMethod");
    }
}
