using MassTransit;
using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Infrastructure.Messaging.MassTransit.Utilities
{
    /// <summary>
    /// Utilities for working with message headers.
    /// </summary>
    public static class MessageHeaderUtilities
    {
        /// <summary>
        /// Gets all headers from a consume context.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="context">The consume context.</param>
        /// <returns>A dictionary of headers.</returns>
        public static IDictionary<string, object> GetAllHeaders<TMessage>(global::MassTransit.ConsumeContext<TMessage> context)
            where TMessage : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var headers = new Dictionary<string, object>();

            foreach (var header in context.Headers.GetAll())
            {
                headers[header.Key] = header.Value ?? string.Empty;
            }

            return headers;
        }

        /// <summary>
        /// Gets a header value from a consume context.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TValue">The type of the header value.</typeparam>
        /// <param name="context">The consume context.</param>
        /// <param name="key">The header key.</param>
        /// <param name="defaultValue">The default value to return if the header is not found.</param>
        /// <returns>The header value.</returns>
        public static TValue GetHeaderValue<TMessage, TValue>(
            global::MassTransit.ConsumeContext<TMessage> context,
            string key,
            TValue? defaultValue = default)
            where TMessage : class
            where TValue : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Header key cannot be null or empty", nameof(key));
            }

            if (context.Headers.TryGetHeader(key, out var value) && value is TValue typedValue)
            {
                return typedValue;
            }

            return defaultValue ?? Activator.CreateInstance<TValue>();
        }

        /// <summary>
        /// Sets headers on a send context.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="context">The send context.</param>
        /// <param name="headers">The headers to set.</param>
        public static void SetHeaders<TMessage>(
            global::MassTransit.SendContext<TMessage> context,
            IDictionary<string, object> headers)
            where TMessage : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            foreach (var header in headers)
            {
                context.Headers.Set(header.Key, header.Value);
            }
        }

        /// <summary>
        /// Sets a header on a send context.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="context">The send context.</param>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        public static void SetHeader<TMessage>(
            global::MassTransit.SendContext<TMessage> context,
            string key,
            object value)
            where TMessage : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Header key cannot be null or empty", nameof(key));
            }

            context.Headers.Set(key, value);
        }
    }
}
