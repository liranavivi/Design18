using FlowOrchestrator.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Mapping
{
    /// <summary>
    /// Mapping configuration for FlowEntity.
    /// </summary>
    public class FlowEntityMap : EntityMap<FlowEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEntityMap"/> class.
        /// </summary>
        public FlowEntityMap() : base()
        {
        }

        /// <inheritdoc />
        protected override void ConfigureMap(BsonClassMap<FlowEntity> cm)
        {
            base.ConfigureMap(cm);

            // Map FlowEntity-specific properties
            cm.GetMemberMap(c => c.FlowId).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Name).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Description).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Owner).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Tags).SetSerializer(new ArraySerializer<string>(new StringSerializer(BsonType.String)));
            cm.GetMemberMap(c => c.ServiceId).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.ServiceType).SetSerializer(new StringSerializer(BsonType.String));
        }
    }
}
