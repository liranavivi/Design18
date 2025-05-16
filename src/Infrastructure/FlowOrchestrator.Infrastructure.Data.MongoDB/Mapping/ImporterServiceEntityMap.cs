using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;

namespace FlowOrchestrator.Infrastructure.Data.MongoDB.Mapping
{
    /// <summary>
    /// Mapping configuration for ImporterServiceEntity.
    /// </summary>
    public class ImporterServiceEntityMap : EntityMap<ImporterServiceEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceEntityMap"/> class.
        /// </summary>
        public ImporterServiceEntityMap() : base()
        {
        }

        /// <inheritdoc />
        protected override void ConfigureMap(BsonClassMap<ImporterServiceEntity> cm)
        {
            base.ConfigureMap(cm);

            // Map ImporterServiceEntity-specific properties
            cm.GetMemberMap(c => c.ServiceId).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Version).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.ServiceType).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Name).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Description).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Status).SetSerializer(new EnumSerializer<FlowOrchestrator.Abstractions.Common.ServiceStatus>(BsonType.String));
            cm.GetMemberMap(c => c.Capabilities).SetSerializer(new ArraySerializer<string>(new StringSerializer(BsonType.String)));
            cm.GetMemberMap(c => c.Protocol).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.Endpoint).SetSerializer(new StringSerializer(BsonType.String));
            cm.GetMemberMap(c => c.SupportedFormats).SetSerializer(new ArraySerializer<string>(new StringSerializer(BsonType.String)));
            cm.GetMemberMap(c => c.MaxFileSizeBytes).SetSerializer(new Int64Serializer(BsonType.Int64));
            cm.GetMemberMap(c => c.PollingIntervalSeconds).SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(c => c.RetryCount).SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(c => c.RetryDelaySeconds).SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(c => c.TimeoutSeconds).SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(c => c.ConcurrencyLimit).SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(c => c.LastHeartbeat).SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            cm.GetMemberMap(c => c.RegistrationTimestamp).SetSerializer(new DateTimeSerializer(BsonType.DateTime));

            // Configuration is a complex type, we'll need to handle it specially
            cm.GetMemberMap(c => c.Configuration).SetSerializer(new ConfigurationParametersSerializer());
        }
    }

    /// <summary>
    /// Custom serializer for ConfigurationParameters.
    /// </summary>
    public class ConfigurationParametersSerializer : SerializerBase<FlowOrchestrator.Abstractions.Common.ConfigurationParameters>
    {
        /// <inheritdoc />
        public override FlowOrchestrator.Abstractions.Common.ConfigurationParameters Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            var parameters = new FlowOrchestrator.Abstractions.Common.ConfigurationParameters();

            reader.ReadStartDocument();
            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = reader.ReadName();
                var bsonType = reader.ReadBsonType();
                object value = null;
                if (bsonType != BsonType.Null)
                {
                    switch (bsonType)
                    {
                        case BsonType.String:
                            value = reader.ReadString();
                            break;
                        case BsonType.Int32:
                            value = reader.ReadInt32();
                            break;
                        case BsonType.Int64:
                            value = reader.ReadInt64();
                            break;
                        case BsonType.Double:
                            value = reader.ReadDouble();
                            break;
                        case BsonType.Boolean:
                            value = reader.ReadBoolean();
                            break;
                        case BsonType.DateTime:
                            value = reader.ReadDateTime();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                else
                {
                    reader.SkipValue();
                }
                parameters.SetParameter(name, value);
            }
            reader.ReadEndDocument();

            return parameters;
        }

        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, FlowOrchestrator.Abstractions.Common.ConfigurationParameters value)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();

            // Get all parameters from the configuration
            var parameters = value.GetAllParameters();
            foreach (var parameter in parameters)
            {
                writer.WriteName(parameter.Key);
                BsonSerializer.Serialize(writer, parameter.Value);
            }

            writer.WriteEndDocument();
        }
    }
}
