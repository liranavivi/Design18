using FlowOrchestrator.Abstractions.Entities;
using System.Text.Json;

namespace FlowOrchestrator.Infrastructure.Data.Hazelcast.Extensions
{
    /// <summary>
    /// Extension methods for entities.
    /// </summary>
    public static class EntityExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Creates a deep copy of an entity.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="entity">The entity to copy.</param>
        /// <returns>A deep copy of the entity.</returns>
        public static T DeepCopy<T>(this T entity) where T : IEntity
        {
            var json = JsonSerializer.Serialize(entity, _jsonOptions);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }
    }
}
