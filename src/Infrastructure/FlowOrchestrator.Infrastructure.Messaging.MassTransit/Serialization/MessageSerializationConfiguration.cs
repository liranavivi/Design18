using MassTransit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Serialization
{
    /// <summary>
    /// Configuration for message serialization.
    /// </summary>
    public static class MessageSerializationConfiguration
    {
        /// <summary>
        /// Configures the default serialization settings for MassTransit.
        /// </summary>
        /// <param name="configurator">The bus factory configurator.</param>
        public static void ConfigureJsonSerialization(IBusFactoryConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.ConfigureJsonSerializerOptions(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
                options.Converters.Add(new JsonStringEnumConverter());
                options.WriteIndented = false;
                return options;
            });
        }

        /// <summary>
        /// Creates a custom JSON serializer settings for MassTransit.
        /// </summary>
        /// <returns>The JSON serializer options.</returns>
        public static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }
    }
}
