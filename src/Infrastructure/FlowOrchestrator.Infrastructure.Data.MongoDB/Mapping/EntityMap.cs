using FlowOrchestrator.Abstractions.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Mapping
{
    /// <summary>
    /// Base class for entity mapping.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public abstract class EntityMap<T> where T : IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMap{T}"/> class.
        /// </summary>
        protected EntityMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    ConfigureMap(cm);
                });
            }
        }

        /// <summary>
        /// Configures the class map.
        /// </summary>
        /// <param name="cm">The class map.</param>
        protected virtual void ConfigureMap(BsonClassMap<T> cm)
        {
            // Map common entity properties
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.GetEntityId()));
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.String));
            cm.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);

            cm.GetMemberMap(c => c.Version).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.CreatedTimestamp).SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            cm.GetMemberMap(c => c.LastModifiedTimestamp).SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            cm.GetMemberMap(c => c.VersionDescription).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.PreviousVersionId).SetSerializer(new StringSerializer(BsonType.String));
        }
    }
}
