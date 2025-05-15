using FlowOrchestrator.Abstractions.Common;
using System.Text.Json;

namespace FlowOrchestrator.Processing.Utilities
{
    /// <summary>
    /// Provides utility methods for processor services.
    /// </summary>
    public static class ProcessorUtilities
    {
        /// <summary>
        /// Creates a new data package with the specified content and content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="schemaVersion">The schema version.</param>
        /// <returns>A new data package.</returns>
        public static DataPackage CreateDataPackage(object content, string contentType, string? schemaId = null, string? schemaVersion = null)
        {
            return new DataPackage
            {
                Content = content,
                ContentType = contentType,
                SchemaId = schemaId,
                SchemaVersion = schemaVersion,
                CreatedTimestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Converts a data package to a JSON string.
        /// </summary>
        /// <param name="dataPackage">The data package to convert.</param>
        /// <returns>The JSON string representation of the data package.</returns>
        public static string ToJsonString(DataPackage dataPackage)
        {
            if (dataPackage == null)
            {
                throw new ArgumentNullException(nameof(dataPackage));
            }

            return JsonSerializer.Serialize(dataPackage.Content);
        }

        /// <summary>
        /// Converts a JSON string to a data package.
        /// </summary>
        /// <param name="jsonString">The JSON string to convert.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="schemaVersion">The schema version.</param>
        /// <returns>A new data package.</returns>
        public static DataPackage FromJsonString(string jsonString, string contentType = "application/json", string? schemaId = null, string? schemaVersion = null)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(jsonString));
            }

            var content = JsonSerializer.Deserialize<JsonElement>(jsonString);
            return CreateDataPackage(content, contentType, schemaId, schemaVersion);
        }

        /// <summary>
        /// Gets a value from a data package by path.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="dataPackage">The data package.</param>
        /// <param name="path">The path to the value.</param>
        /// <returns>The value at the specified path.</returns>
        public static T? GetValueByPath<T>(DataPackage dataPackage, string path)
        {
            if (dataPackage == null)
            {
                throw new ArgumentNullException(nameof(dataPackage));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            // Convert content to JSON element if it's not already
            JsonElement rootElement;
            if (dataPackage.Content is JsonElement element)
            {
                rootElement = element;
            }
            else
            {
                var jsonString = JsonSerializer.Serialize(dataPackage.Content);
                rootElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
            }

            // Split the path by dots
            var pathParts = path.Split('.');
            var currentElement = rootElement;

            // Navigate through the path
            foreach (var part in pathParts)
            {
                if (currentElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidOperationException($"Cannot navigate to '{part}' because the current element is not an object.");
                }

                if (!currentElement.TryGetProperty(part, out var property))
                {
                    throw new InvalidOperationException($"Property '{part}' not found in the current element.");
                }

                currentElement = property;
            }

            // Convert the final element to the requested type
            return JsonSerializer.Deserialize<T>(currentElement.GetRawText());
        }

        /// <summary>
        /// Sets a value in a data package by path.
        /// </summary>
        /// <param name="dataPackage">The data package.</param>
        /// <param name="path">The path to the value.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A new data package with the updated value.</returns>
        public static DataPackage SetValueByPath(DataPackage dataPackage, string path, object value)
        {
            if (dataPackage == null)
            {
                throw new ArgumentNullException(nameof(dataPackage));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            // Convert content to dictionary if it's not already
            Dictionary<string, object> rootObject;
            if (dataPackage.Content is Dictionary<string, object> dict)
            {
                rootObject = dict;
            }
            else
            {
                var jsonString = JsonSerializer.Serialize(dataPackage.Content);
                rootObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString) ?? new Dictionary<string, object>();
            }

            // Split the path by dots
            var pathParts = path.Split('.');
            var currentObject = rootObject;

            // Navigate through the path
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var part = pathParts[i];

                if (!currentObject.ContainsKey(part))
                {
                    currentObject[part] = new Dictionary<string, object>();
                }

                if (currentObject[part] is not Dictionary<string, object> nestedObject)
                {
                    currentObject[part] = new Dictionary<string, object>();
                    nestedObject = (Dictionary<string, object>)currentObject[part];
                }

                currentObject = nestedObject;
            }

            // Set the value at the final path part
            currentObject[pathParts[^1]] = value;

            // Create a new data package with the updated content
            return new DataPackage
            {
                Content = rootObject,
                ContentType = dataPackage.ContentType,
                SchemaId = dataPackage.SchemaId,
                SchemaVersion = dataPackage.SchemaVersion,
                CreatedTimestamp = DateTime.UtcNow,
                Source = dataPackage.Source,
                Metadata = dataPackage.Metadata,
                ValidationResults = dataPackage.ValidationResults
            };
        }

        /// <summary>
        /// Merges two data packages.
        /// </summary>
        /// <param name="primary">The primary data package.</param>
        /// <param name="secondary">The secondary data package.</param>
        /// <returns>A new data package with the merged content.</returns>
        public static DataPackage MergeDataPackages(DataPackage primary, DataPackage secondary)
        {
            if (primary == null)
            {
                throw new ArgumentNullException(nameof(primary));
            }

            if (secondary == null)
            {
                throw new ArgumentNullException(nameof(secondary));
            }

            // Convert both contents to dictionaries
            Dictionary<string, object> primaryDict;
            Dictionary<string, object> secondaryDict;

            if (primary.Content is Dictionary<string, object> dict1)
            {
                primaryDict = dict1;
            }
            else
            {
                var jsonString1 = JsonSerializer.Serialize(primary.Content);
                primaryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString1) ?? new Dictionary<string, object>();
            }

            if (secondary.Content is Dictionary<string, object> dict2)
            {
                secondaryDict = dict2;
            }
            else
            {
                var jsonString2 = JsonSerializer.Serialize(secondary.Content);
                secondaryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString2) ?? new Dictionary<string, object>();
            }

            // Merge the dictionaries
            var mergedDict = new Dictionary<string, object>(primaryDict);
            foreach (var kvp in secondaryDict)
            {
                if (!mergedDict.ContainsKey(kvp.Key))
                {
                    mergedDict[kvp.Key] = kvp.Value;
                }
                else if (mergedDict[kvp.Key] is Dictionary<string, object> nestedPrimary && 
                         kvp.Value is Dictionary<string, object> nestedSecondary)
                {
                    // Recursively merge nested dictionaries
                    MergeDictionaries(nestedPrimary, nestedSecondary);
                }
                // Otherwise, primary value takes precedence
            }

            // Create a new data package with the merged content
            return new DataPackage
            {
                Content = mergedDict,
                ContentType = primary.ContentType,
                SchemaId = primary.SchemaId,
                SchemaVersion = primary.SchemaVersion,
                CreatedTimestamp = DateTime.UtcNow,
                Source = primary.Source,
                Metadata = MergeDictionaries(primary.Metadata, secondary.Metadata),
                ValidationResults = primary.ValidationResults
            };
        }

        /// <summary>
        /// Merges two dictionaries.
        /// </summary>
        /// <param name="primary">The primary dictionary.</param>
        /// <param name="secondary">The secondary dictionary.</param>
        /// <returns>The merged dictionary.</returns>
        private static Dictionary<string, object> MergeDictionaries(Dictionary<string, object> primary, Dictionary<string, object> secondary)
        {
            var result = new Dictionary<string, object>(primary);

            foreach (var kvp in secondary)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    result[kvp.Key] = kvp.Value;
                }
                else if (result[kvp.Key] is Dictionary<string, object> nestedPrimary && 
                         kvp.Value is Dictionary<string, object> nestedSecondary)
                {
                    // Recursively merge nested dictionaries
                    result[kvp.Key] = MergeDictionaries(nestedPrimary, nestedSecondary);
                }
                // Otherwise, primary value takes precedence
            }

            return result;
        }
    }
}
