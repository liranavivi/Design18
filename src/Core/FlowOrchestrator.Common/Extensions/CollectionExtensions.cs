namespace FlowOrchestrator.Common.Extensions;

/// <summary>
/// Provides extension methods for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Returns a collection as an enumerable or an empty enumerable if the collection is null.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <returns>The collection as an enumerable or an empty enumerable if the collection is null.</returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Adds a range of items to a collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="items">The items to add.</param>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (items == null)
        {
            return;
        }

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Adds an item to a collection if it is not null.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="item">The item to add.</param>
    /// <returns>True if the item was added; otherwise, false.</returns>
    public static bool AddIfNotNull<T>(this ICollection<T> collection, T? item) where T : class
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (item == null)
        {
            return false;
        }

        collection.Add(item);
        return true;
    }

    /// <summary>
    /// Adds an item to a collection if it satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="condition">The condition to check.</param>
    /// <returns>True if the item was added; otherwise, false.</returns>
    public static bool AddIf<T>(this ICollection<T> collection, T item, Func<T, bool> condition)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (condition(item))
        {
            collection.Add(item);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all items from a collection that satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="condition">The condition to check.</param>
    /// <returns>The number of items removed.</returns>
    public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> condition)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        var itemsToRemove = collection.Where(condition).ToList();
        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }

        return itemsToRemove.Count;
    }

    /// <summary>
    /// Performs an action on each item in a collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to perform.</param>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        foreach (var item in collection)
        {
            action(item);
        }
    }
}
