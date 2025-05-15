using FlowOrchestrator.Abstractions.Common;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace FlowOrchestrator.Integration.Importers.File.Utilities
{
    /// <summary>
    /// Utility class for parsing different file formats.
    /// </summary>
    public static class FileParserUtility
    {
        /// <summary>
        /// Parses file content based on the content type.
        /// </summary>
        /// <param name="content">The file content.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The parsed content.</returns>
        public static object ParseContent(object content, string contentType)
        {
            if (content == null)
            {
                return new object();
            }

            // If content is already parsed, return it
            if (content is not byte[] && content is not string)
            {
                return content;
            }

            try
            {
                // Convert byte array to string if needed
                string stringContent = content is byte[] byteContent
                    ? Encoding.UTF8.GetString(byteContent)
                    : content.ToString() ?? string.Empty;

                // Parse based on content type
                if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseJson(stringContent);
                }
                else if (contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseXml(stringContent);
                }
                else if (contentType.Contains("csv", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseCsv(stringContent);
                }
                else if (contentType.Contains("text", StringComparison.OrdinalIgnoreCase))
                {
                    return stringContent;
                }
                else
                {
                    // For binary data, return as is
                    return content;
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse content as {contentType}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses JSON content.
        /// </summary>
        /// <param name="content">The JSON content.</param>
        /// <returns>The parsed JSON object.</returns>
        private static object ParseJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new object();
            }

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(content);
            }
            catch (JsonException)
            {
                // If parsing fails, return the raw string
                return content;
            }
        }

        /// <summary>
        /// Parses XML content.
        /// </summary>
        /// <param name="content">The XML content.</param>
        /// <returns>The parsed XML document.</returns>
        private static object ParseXml(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new object();
            }

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);
                return xmlDoc;
            }
            catch (XmlException)
            {
                // If parsing fails, return the raw string
                return content;
            }
        }

        /// <summary>
        /// Parses CSV content.
        /// </summary>
        /// <param name="content">The CSV content.</param>
        /// <returns>The parsed CSV data as a list of dictionaries.</returns>
        private static object ParseCsv(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<Dictionary<string, string>>();
            }

            try
            {
                var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                {
                    return new List<Dictionary<string, string>>();
                }

                // Parse header
                var headers = ParseCsvLine(lines[0]);

                // Parse data rows
                var data = new List<Dictionary<string, string>>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    var row = new Dictionary<string, string>();

                    for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
                    {
                        row[headers[j]] = values[j];
                    }

                    data.Add(row);
                }

                return data;
            }
            catch (Exception)
            {
                // If parsing fails, return the raw string
                return content;
            }
        }

        /// <summary>
        /// Parses a CSV line.
        /// </summary>
        /// <param name="line">The CSV line.</param>
        /// <returns>The parsed values.</returns>
        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            StringBuilder currentValue = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        // Toggle quotes
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of value
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    // Add character to current value
                    currentValue.Append(c);
                }
            }

            // Add the last value
            values.Add(currentValue.ToString());

            return values;
        }

        /// <summary>
        /// Enhances a data package with parsed content.
        /// </summary>
        /// <param name="dataPackage">The data package to enhance.</param>
        /// <returns>The enhanced data package.</returns>
        public static DataPackage EnhanceDataPackage(DataPackage dataPackage)
        {
            if (dataPackage == null || dataPackage.Content == null)
            {
                return dataPackage ?? new DataPackage();
            }

            // Parse the content based on the content type
            var parsedContent = ParseContent(dataPackage.Content, dataPackage.ContentType);
            
            // Update the data package with the parsed content
            dataPackage.Content = parsedContent;

            return dataPackage;
        }
    }
}
