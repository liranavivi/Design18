using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace Shared.Configuration;

/// <summary>
/// Configuration extension methods for MassTransit setup with RabbitMQ.
/// </summary>
public static class MassTransitConfiguration
{
    /// <summary>
    /// Adds MassTransit with RabbitMQ configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="consumerTypes">The consumer types to register.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services, 
        IConfiguration configuration, 
        params Type[] consumerTypes)
    {
        services.AddMassTransit(x =>
        {
            // Add consumers dynamically
            foreach (var consumerType in consumerTypes)
            {
                x.AddConsumer(consumerType);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitMqSettings["Host"] ?? "localhost", rabbitMqSettings["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitMqSettings["Username"] ?? "guest");
                    h.Password(rabbitMqSettings["Password"] ?? "guest");
                });

                // Configure retry policy
                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(30)
                ));

                // Configure error handling
                // cfg.UseInMemoryOutbox(); // Commented out due to obsolete warning

                // Configure endpoints to use message type routing
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
