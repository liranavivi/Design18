using FlowOrchestrator.Common.Extensions;

namespace FlowOrchestrator.Common.Tests.Extensions;

public class CollectionExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(new int[0], true)]
    [InlineData(new[] { 1 }, false)]
    public void IsNullOrEmpty_ReturnsExpectedResult(IEnumerable<int>? collection, bool expected)
    {
        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EmptyIfNull_WithNullCollection_ReturnsEmptyEnumerable()
    {
        // Arrange
        IEnumerable<int>? collection = null;

        // Act
        var result = collection.EmptyIfNull();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void EmptyIfNull_WithNonNullCollection_ReturnsSameCollection()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act
        var result = collection.EmptyIfNull();

        // Assert
        Assert.Same(collection, result);
    }

    [Fact]
    public void AddRange_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? collection = null;
        var items = new[] { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.AddRange(items));
    }

    [Fact]
    public void AddRange_WithNullItems_DoesNotModifyCollection()
    {
        // Arrange
        // Use a custom collection that doesn't have its own AddRange method
        ICollection<int> collection = new HashSet<int> { 1, 2, 3 };
        var originalCount = collection.Count;
        IEnumerable<int>? items = null;

        // Act
        // The custom extension method will handle the null check
        collection.AddRange(items!);

        // Assert
        Assert.Equal(originalCount, collection.Count);
    }

    [Fact]
    public void AddRange_WithValidCollectionAndItems_AddsAllItems()
    {
        // Arrange
        var collection = new List<int>();
        var items = new[] { 1, 2, 3 };

        // Act
        collection.AddRange(items);

        // Assert
        Assert.Equal(items.Length, collection.Count);
        Assert.Equal(items, collection);
    }

    [Fact]
    public void AddIfNotNull_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<string>? collection = null;
        string item = "test";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.AddIfNotNull(item));
    }

    [Fact]
    public void AddIfNotNull_WithNullItem_ReturnsFalseAndDoesNotAddItem()
    {
        // Arrange
        var collection = new List<string>();
        string? item = null;

        // Act
        var result = collection.AddIfNotNull(item);

        // Assert
        Assert.False(result);
        Assert.Empty(collection);
    }

    [Fact]
    public void AddIfNotNull_WithNonNullItem_ReturnsTrueAndAddsItem()
    {
        // Arrange
        var collection = new List<string>();
        var item = "test";

        // Act
        var result = collection.AddIfNotNull(item);

        // Assert
        Assert.True(result);
        Assert.Single(collection);
        Assert.Equal(item, collection[0]);
    }

    [Fact]
    public void AddIf_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? collection = null;
        int item = 1;
        Func<int, bool> condition = i => i > 0;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.AddIf(item, condition));
    }

    [Fact]
    public void AddIf_WithNullCondition_ThrowsArgumentNullException()
    {
        // Arrange
        var collection = new List<int>();
        int item = 1;
        Func<int, bool>? condition = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.AddIf(item, condition!));
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(0, false)]
    public void AddIf_WithCondition_ReturnsExpectedResultAndAddsItemIfConditionIsMet(int item, bool shouldAdd)
    {
        // Arrange
        var collection = new List<int>();
        Func<int, bool> condition = i => i > 0;

        // Act
        var result = collection.AddIf(item, condition);

        // Assert
        Assert.Equal(shouldAdd, result);
        Assert.Equal(shouldAdd ? 1 : 0, collection.Count);
        if (shouldAdd)
        {
            Assert.Equal(item, collection[0]);
        }
    }

    [Fact]
    public void RemoveAll_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? collection = null;
        Func<int, bool> condition = i => i > 0;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.RemoveAll(condition));
    }

    [Fact]
    public void RemoveAll_WithNullCondition_ThrowsArgumentNullException()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };
        Func<int, bool>? condition = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.RemoveAll(condition!));
    }

    [Fact]
    public void RemoveAll_WithCondition_RemovesMatchingItemsAndReturnsCount()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3, 4, 5 };
        Func<int, bool> condition = i => i % 2 == 0;

        // Act
        var result = collection.RemoveAll(condition);

        // Assert
        Assert.Equal(2, result);
        Assert.Equal(new[] { 1, 3, 5 }, collection);
    }

    [Fact]
    public void ForEach_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? collection = null;
        Action<int> action = i => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.ForEach(action));
    }

    [Fact]
    public void ForEach_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };
        Action<int>? action = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.ForEach(action!));
    }

    [Fact]
    public void ForEach_WithValidCollectionAndAction_PerformsActionOnEachItem()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };
        var sum = 0;
        Action<int> action = i => sum += i;

        // Act
        collection.ForEach(action);

        // Assert
        Assert.Equal(6, sum);
    }
}
