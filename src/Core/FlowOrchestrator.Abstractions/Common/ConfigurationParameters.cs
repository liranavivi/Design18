using System.Collections;

namespace FlowOrchestrator.Abstractions.Common
{
    /// <summary>
    /// Represents configuration parameters for services in the FlowOrchestrator system.
    /// </summary>
    public class ConfigurationParameters : IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// Gets or sets the dictionary of configuration parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Checks if the parameters dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return Parameters.ContainsKey(key);
        }

        /// <summary>
        /// Gets a parameter value by key.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <returns>The parameter value.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
        /// <exception cref="InvalidCastException">Thrown when the value cannot be cast to the specified type.</exception>
        public T GetParameter<T>(string key)
        {
            if (!Parameters.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Configuration parameter '{key}' not found.");
            }

            try
            {
                return (T)Parameters[key];
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Configuration parameter '{key}' cannot be cast to type {typeof(T).Name}.");
            }
        }

        /// <summary>
        /// Tries to get a parameter value by key.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value if found and of the correct type; otherwise, the default value for the type.</param>
        /// <returns>true if the parameter was found and is of the correct type; otherwise, false.</returns>
        public bool TryGetParameter<T>(string key, out T value)
        {
            value = default!;

            if (!Parameters.ContainsKey(key))
            {
                return false;
            }

            try
            {
                value = (T)Parameters[key];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a parameter value by key, or a default value if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>The parameter value if found and of the correct type; otherwise, the default value.</returns>
        public T GetParameterOrDefault<T>(string key, T defaultValue)
        {
            if (TryGetParameter<T>(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        public void SetParameter<T>(string key, T value)
        {
            Parameters[key] = value!;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
