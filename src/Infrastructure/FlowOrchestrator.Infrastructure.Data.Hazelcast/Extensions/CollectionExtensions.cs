namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Extensions
{
    /// <summary>
    /// Extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Converts an IReadOnlyCollection to an ICollection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns>An ICollection containing the elements from the source collection.</returns>
        public static ICollection<T> ToCollection<T>(this IReadOnlyCollection<T> source)
        {
            return new List<T>(source);
        }
    }
}
