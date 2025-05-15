using FlowOrchestrator.Processing.Json.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlowOrchestrator.Processing.Json.Utilities
{
    /// <summary>
    /// Provides JSON transformation functionality.
    /// </summary>
    public class JsonTransformationEngine
    {
        /// <summary>
        /// Transforms JSON data based on the specified options.
        /// </summary>
        /// <param name="jsonData">The JSON data to transform.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The transformed JSON data.</returns>
        public static JsonNode TransformJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            if (jsonData == null)
            {
                throw new ArgumentNullException(nameof(jsonData));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return options.TransformationType switch
            {
                TransformationType.Map => MapJson(jsonData, options),
                TransformationType.Filter => FilterJson(jsonData, options),
                TransformationType.Extract => ExtractJson(jsonData, options),
                TransformationType.Merge => jsonData, // Merge is handled separately
                TransformationType.Flatten => FlattenJson(jsonData, options),
                TransformationType.Custom => ApplyCustomTransformations(jsonData, options),
                _ => throw new ArgumentException($"Unsupported transformation type: {options.TransformationType}")
            };
        }

        /// <summary>
        /// Maps JSON data based on field mappings.
        /// </summary>
        /// <param name="jsonData">The JSON data to map.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The mapped JSON data.</returns>
        private static JsonNode MapJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            var result = new JsonObject();

            if (jsonData is JsonObject sourceObject)
            {
                foreach (var mapping in options.FieldMappings)
                {
                    var sourcePath = mapping.Key;
                    var targetPath = mapping.Value;

                    // Extract value from source path
                    var value = GetValueByPath(sourceObject, sourcePath);

                    if (value != null || options.PreserveNullValues)
                    {
                        // Set value at target path
                        SetValueByPath(result, targetPath, value);
                    }
                }

                // Add default values for missing fields
                foreach (var defaultValue in options.DefaultValues)
                {
                    var path = defaultValue.Key;

                    // Check if the path already exists
                    if (GetValueByPath(result, path) == null)
                    {
                        SetValueByPath(result, path, JsonValue.Create(defaultValue.Value));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Filters JSON data based on include/exclude fields.
        /// </summary>
        /// <param name="jsonData">The JSON data to filter.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The filtered JSON data.</returns>
        private static JsonNode FilterJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            if (jsonData is JsonObject sourceObject)
            {
                var result = new JsonObject();

                // If include fields are specified, only include those fields
                if (options.IncludeFields.Count > 0)
                {
                    foreach (var field in options.IncludeFields)
                    {
                        if (sourceObject.ContainsKey(field))
                        {
                            result[field] = sourceObject[field]?.DeepClone();
                        }
                    }
                }
                // Otherwise, include all fields except those in exclude fields
                else
                {
                    foreach (var property in sourceObject)
                    {
                        if (!options.ExcludeFields.Contains(property.Key))
                        {
                            result[property.Key] = property.Value?.DeepClone();
                        }
                    }
                }

                return result;
            }
            else if (jsonData is JsonArray sourceArray)
            {
                var result = new JsonArray();

                foreach (var item in sourceArray)
                {
                    if (item != null)
                    {
                        result.Add(FilterJson(item, options));
                    }
                    else if (options.PreserveNullValues)
                    {
                        result.Add(null);
                    }
                }

                return result;
            }

            return jsonData.DeepClone();
        }

        /// <summary>
        /// Extracts JSON data based on JSON path expressions.
        /// </summary>
        /// <param name="jsonData">The JSON data to extract from.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The extracted JSON data.</returns>
        private static JsonNode ExtractJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            var result = new JsonObject();

            foreach (var extraction in options.PathExpressions)
            {
                var targetField = extraction.Key;
                var sourcePath = extraction.Value;

                // Extract value from source path
                var value = GetValueByPath(jsonData, sourcePath);

                if (value != null || options.PreserveNullValues)
                {
                    result[targetField] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// Flattens nested JSON objects.
        /// </summary>
        /// <param name="jsonData">The JSON data to flatten.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The flattened JSON data.</returns>
        private static JsonNode FlattenJson(JsonNode jsonData, JsonTransformationOptions options)
        {
            var result = new JsonObject();
            FlattenJsonRecursive(jsonData, "", options.FlattenDelimiter, result, options);
            return result;
        }

        /// <summary>
        /// Recursively flattens nested JSON objects.
        /// </summary>
        /// <param name="jsonData">The JSON data to flatten.</param>
        /// <param name="prefix">The current path prefix.</param>
        /// <param name="delimiter">The delimiter for flattened field names.</param>
        /// <param name="result">The result object to populate.</param>
        /// <param name="options">The transformation options.</param>
        private static void FlattenJsonRecursive(JsonNode? jsonData, string prefix, string delimiter, JsonObject result, JsonTransformationOptions options)
        {
            if (jsonData == null)
            {
                return;
            }

            if (jsonData is JsonObject obj)
            {
                // If it's an empty object and we're preserving empty objects
                if (obj.Count == 0 && options.PreserveEmptyObjects && !string.IsNullOrEmpty(prefix))
                {
                    result[prefix] = new JsonObject();
                    return;
                }

                foreach (var property in obj)
                {
                    var newPrefix = string.IsNullOrEmpty(prefix) ? property.Key : $"{prefix}{delimiter}{property.Key}";
                    FlattenJsonRecursive(property.Value, newPrefix, delimiter, result, options);
                }
            }
            else if (jsonData is JsonArray array)
            {
                // Handle arrays based on the array handling mode
                switch (options.ArrayHandlingMode)
                {
                    case ArrayHandlingMode.Preserve:
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            result[prefix] = array.DeepClone();
                        }
                        break;

                    case ArrayHandlingMode.Transform:
                        if (array.Count == 0 && options.PreserveEmptyArrays && !string.IsNullOrEmpty(prefix))
                        {
                            result[prefix] = new JsonArray();
                        }
                        else
                        {
                            for (int i = 0; i < array.Count; i++)
                            {
                                var newPrefix = $"{prefix}{delimiter}{i}";
                                FlattenJsonRecursive(array[i], newPrefix, delimiter, result, options);
                            }
                        }
                        break;

                    case ArrayHandlingMode.Flatten:
                        if (array.Count == 0 && options.PreserveEmptyArrays && !string.IsNullOrEmpty(prefix))
                        {
                            result[prefix] = new JsonArray();
                        }
                        else
                        {
                            for (int i = 0; i < array.Count; i++)
                            {
                                FlattenJsonRecursive(array[i], prefix, delimiter, result, options);
                            }
                        }
                        break;
                }
            }
            else
            {
                // It's a primitive value
                if (!string.IsNullOrEmpty(prefix))
                {
                    result[prefix] = jsonData.DeepClone();
                }
            }
        }

        /// <summary>
        /// Applies custom transformations to JSON data.
        /// </summary>
        /// <param name="jsonData">The JSON data to transform.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The transformed JSON data.</returns>
        private static JsonNode ApplyCustomTransformations(JsonNode jsonData, JsonTransformationOptions options)
        {
            if (jsonData is JsonObject sourceObject)
            {
                var result = sourceObject.DeepClone();

                foreach (var transformation in options.FieldTransformations)
                {
                    var field = transformation.Key;
                    var transformType = transformation.Value;

                    if (result[field] is JsonNode value)
                    {
                        // Apply the transformation based on the transform type
                        var transformedValue = ApplyTransformation(value, transformType);
                        result[field] = transformedValue;
                    }
                }

                return result;
            }

            return jsonData.DeepClone();
        }

        /// <summary>
        /// Applies a transformation to a JSON value.
        /// </summary>
        /// <param name="value">The value to transform.</param>
        /// <param name="transformType">The type of transformation to apply.</param>
        /// <returns>The transformed value.</returns>
        private static JsonNode? ApplyTransformation(JsonNode? value, string transformType)
        {
            if (value == null)
            {
                return null;
            }

            // Simple transformations for demonstration purposes
            return transformType.ToLowerInvariant() switch
            {
                "uppercase" => JsonValue.Create(value.GetValue<string>().ToUpperInvariant()),
                "lowercase" => JsonValue.Create(value.GetValue<string>().ToLowerInvariant()),
                "trim" => JsonValue.Create(value.GetValue<string>().Trim()),
                "toint" => JsonValue.Create(Convert.ToInt32(value.GetValue<string>())),
                "tostring" => JsonValue.Create(value.ToString()),
                "tobool" => JsonValue.Create(Convert.ToBoolean(value.GetValue<string>())),
                _ => value.DeepClone()
            };
        }

        /// <summary>
        /// Gets a value from a JSON object by path.
        /// </summary>
        /// <param name="jsonData">The JSON data to get the value from.</param>
        /// <param name="path">The path to the value.</param>
        /// <returns>The value at the specified path, or null if not found.</returns>
        private static JsonNode? GetValueByPath(JsonNode jsonData, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return jsonData.DeepClone();
            }

            var pathParts = path.Split('.');
            var current = jsonData;

            foreach (var part in pathParts)
            {
                if (current is JsonObject obj)
                {
                    if (!obj.TryGetPropertyValue(part, out var value))
                    {
                        return null;
                    }
                    current = value;
                }
                else
                {
                    return null;
                }
            }

            return current?.DeepClone();
        }

        /// <summary>
        /// Sets a value in a JSON object by path.
        /// </summary>
        /// <param name="jsonObject">The JSON object to set the value in.</param>
        /// <param name="path">The path to set the value at.</param>
        /// <param name="value">The value to set.</param>
        private static void SetValueByPath(JsonObject jsonObject, string path, JsonNode? value)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var pathParts = path.Split('.');
            var current = jsonObject;

            // Navigate to the parent object
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                var part = pathParts[i];

                if (!current.TryGetPropertyValue(part, out var child) || !(child is JsonObject))
                {
                    // Create intermediate objects if they don't exist
                    var newObject = new JsonObject();
                    current[part] = newObject;
                    current = newObject;
                }
                else
                {
                    current = (JsonObject)child;
                }
            }

            // Set the value in the parent object
            current[pathParts[^1]] = value;
        }

        /// <summary>
        /// Merges multiple JSON objects.
        /// </summary>
        /// <param name="jsonObjects">The JSON objects to merge.</param>
        /// <param name="options">The transformation options.</param>
        /// <returns>The merged JSON object.</returns>
        public static JsonObject MergeJsonObjects(IEnumerable<JsonNode> jsonObjects, JsonTransformationOptions options)
        {
            var result = new JsonObject();

            foreach (var jsonObject in jsonObjects)
            {
                if (jsonObject is JsonObject obj)
                {
                    MergeObjects(result, obj, options);
                }
            }

            return result;
        }

        /// <summary>
        /// Merges two JSON objects.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="source">The source object.</param>
        /// <param name="options">The transformation options.</param>
        private static void MergeObjects(JsonObject target, JsonObject source, JsonTransformationOptions options)
        {
            foreach (var property in source)
            {
                if (!target.ContainsKey(property.Key))
                {
                    // Property doesn't exist in target, add it
                    target[property.Key] = property.Value?.DeepClone();
                }
                else if (target[property.Key] is JsonObject targetObj && property.Value is JsonObject sourceObj)
                {
                    // Both are objects, merge recursively
                    MergeObjects(targetObj, sourceObj, options);
                }
                else if (target[property.Key] is JsonArray targetArray && property.Value is JsonArray sourceArray)
                {
                    // Both are arrays, merge based on array handling mode
                    MergeArrays(targetArray, sourceArray, options);
                }
                else
                {
                    // Different types or primitive values, overwrite target
                    target[property.Key] = property.Value?.DeepClone();
                }
            }
        }

        /// <summary>
        /// Merges two JSON arrays.
        /// </summary>
        /// <param name="target">The target array.</param>
        /// <param name="source">The source array.</param>
        /// <param name="options">The transformation options.</param>
        private static void MergeArrays(JsonArray target, JsonArray source, JsonTransformationOptions options)
        {
            // Simple array merging - just append source items to target
            foreach (var item in source)
            {
                target.Add(item?.DeepClone());
            }
        }
    }
}
