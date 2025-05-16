using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Abstractions;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Adapters;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Configuration;
using FlowOrchestrator.Infrastructure.Messaging.MassTransit.Implementation;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Extensions
{
    /// <summary>
    /// Extension methods for registering MassTransit services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MassTransit message bus services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFlowOrchestratorMessageBus(
            this IServiceCollection services,
            Action<MessageBusOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var options = new MessageBusOptions();
            configureOptions(options);

            services.AddSingleton(options);
            services.TryAddSingleton<IMessageBus, MassTransitMessageBus>();

            services.AddMassTransit(busConfigurator =>
            {
                ConfigureTransport(busConfigurator, options);
            });

            return services;
        }

        /// <summary>
        /// Registers a FlowOrchestrator message consumer with MassTransit.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFlowOrchestratorConsumer<TMessage, TConsumer>(this IServiceCollection services)
            where TMessage : class
            where TConsumer : class, IMessageConsumer<TMessage>
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddScoped<TConsumer>();
            services.TryAddScoped<IMessageConsumer<TMessage>>(sp => sp.GetRequiredService<TConsumer>());
            services.TryAddScoped<ConsumerAdapter<TMessage>>();
            services.AddMassTransit(x => x.AddConsumer<ConsumerAdapter<TMessage>>());

            return services;
        }

        private static void ConfigureTransport(IBusRegistrationConfigurator busConfigurator, MessageBusOptions options)
        {
            switch (options.TransportType)
            {
                case TransportType.InMemory:
                    busConfigurator.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                        ConfigureCommonOptions(cfg, options);
                    });
                    break;

                case TransportType.RabbitMq:
                    if (string.IsNullOrEmpty(options.HostAddress))
                    {
                        throw new ArgumentException("Host address is required for RabbitMQ transport");
                    }

                    busConfigurator.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(new Uri(options.HostAddress), h =>
                        {
                            // Virtual host is set as part of the URI in RabbitMQ

                            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                            {
                                h.Username(options.Username);
                                h.Password(options.Password);
                            }
                        });

                        cfg.ConfigureEndpoints(context);
                        ConfigureCommonOptions(cfg, options);
                    });
                    break;

                case TransportType.AzureServiceBus:
                    if (string.IsNullOrEmpty(options.HostAddress))
                    {
                        throw new ArgumentException("Host address is required for Azure Service Bus transport");
                    }

                    busConfigurator.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(options.HostAddress, h =>
                        {
                            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                            {
                                // Use Azure.Identity for authentication
                                h.ConnectionString = $"Endpoint={options.HostAddress};SharedAccessKeyName={options.Username};SharedAccessKey={options.Password}";
                            }
                        });

                        cfg.ConfigureEndpoints(context);
                        ConfigureCommonOptions(cfg, options);
                    });
                    break;

                default:
                    throw new ArgumentException($"Unsupported transport type: {options.TransportType}");
            }
        }

        private static void ConfigureCommonOptions<T>(IBusFactoryConfigurator<T> configurator, MessageBusOptions options)
            where T : IReceiveEndpointConfigurator
        {
            configurator.UseMessageRetry(r => r.Interval(options.RetryCount, TimeSpan.FromSeconds(options.RetryIntervalSeconds)));
            configurator.PrefetchCount = options.PrefetchCount;
            configurator.ConcurrentMessageLimit = options.ConcurrencyLimit;
        }
    }
}
