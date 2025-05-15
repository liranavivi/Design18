using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Integration.Protocols.Utilities;
using System.Text;

namespace FlowOrchestrator.Integration.Protocols.Protocols
{
    /// <summary>
    /// Implementation of the file protocol handler.
    /// </summary>
    public class FileProtocolHandler : AbstractProtocolHandler
    {
        private readonly string _basePath;
        private readonly string _filePattern;
        private readonly bool _recursive;
        private readonly Encoding _encoding;
        private readonly int _bufferSize;
        private bool _isConnected;

        /// <summary>
        /// Gets the protocol name.
        /// </summary>
        public override string ProtocolName => "file";

        /// <summary>
        /// Gets a value indicating whether the handler is connected.
        /// </summary>
        public override bool IsConnected => _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProtocolHandler"/> class.
        /// </summary>
        /// <param name="parameters">The connection parameters.</param>
        public FileProtocolHandler(Dictionary<string, string> parameters)
            : base(parameters)
        {
            _basePath = GetParameterValue("basePath", string.Empty);
            _filePattern = GetParameterValue("filePattern", "*.*");
            _recursive = GetParameterValue("recursive", false);
            _bufferSize = GetParameterValue("bufferSize", 4096);

            // Parse encoding
            var encodingName = GetParameterValue("encoding", "utf-8");
            try
            {
                _encoding = Encoding.GetEncoding(encodingName);
            }
            catch
            {
                _encoding = Encoding.UTF8;
            }
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <returns>true if the connection was opened successfully; otherwise, false.</returns>
        public override bool Connect()
        {
            if (_isConnected)
            {
                return true;
            }

            try
            {
                // Validate that the base path exists
                if (!Directory.Exists(_basePath))
                {
                    return false;
                }

                _isConnected = true;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public override void Disconnect()
        {
            _isConnected = false;
        }

        /// <summary>
        /// Reads data from the connection.
        /// </summary>
        /// <param name="options">The read options.</param>
        /// <returns>The data read from the connection.</returns>
        public override DataPackage Read(Dictionary<string, object> options)
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Not connected.");
            }

            if (!ValidateReadOptions(options))
            {
                throw new ArgumentException("Invalid read options.");
            }

            try
            {
                // Get file path from options or use pattern to find files
                var filePath = GetOptionValue(options, "filePath", string.Empty);

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    // Read a specific file
                    return ReadFile(filePath);
                }
                else
                {
                    // Find files matching the pattern
                    var files = FindFiles();

                    // Read the first file if any
                    if (files.Length > 0)
                    {
                        return ReadFile(files[0]);
                    }
                    else
                    {
                        throw new FileNotFoundException("No files found matching the pattern.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes data to the connection.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="options">The write options.</param>
        /// <returns>true if the data was written successfully; otherwise, false.</returns>
        public override bool Write(DataPackage data, Dictionary<string, object> options)
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Not connected.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!ValidateWriteOptions(options))
            {
                throw new ArgumentException("Invalid write options.");
            }

            try
            {
                // Get file path from options
                var filePath = GetOptionValue(options, "filePath", string.Empty);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("File path is required for write operations.");
                }

                // Combine with base path if not absolute
                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.Combine(_basePath, filePath);
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Convert data to bytes
                var bytes = ProtocolUtilities.ToBytes(data);

                // Write to file
                File.WriteAllBytes(filePath, bytes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Classifies an exception into an error code.
        /// </summary>
        /// <param name="ex">The exception to classify.</param>
        /// <returns>The error code.</returns>
        protected override string ClassifyException(Exception ex)
        {
            if (ex is FileNotFoundException)
            {
                return "FILE_NOT_FOUND";
            }
            else if (ex is DirectoryNotFoundException)
            {
                return "DIRECTORY_NOT_FOUND";
            }
            else if (ex is IOException)
            {
                return "IO_ERROR";
            }
            else if (ex is UnauthorizedAccessException)
            {
                return "ACCESS_DENIED";
            }
            else if (ex is ArgumentException)
            {
                return "INVALID_ARGUMENT";
            }
            else
            {
                return "GENERAL_ERROR";
            }
        }

        /// <summary>
        /// Gets detailed information about an exception.
        /// </summary>
        /// <param name="ex">The exception to get details for.</param>
        /// <returns>The error details.</returns>
        protected override Dictionary<string, object> GetErrorDetails(Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "message", ex.Message },
                { "stackTrace", ex.StackTrace ?? string.Empty },
                { "source", ex.Source ?? string.Empty },
                { "type", ex.GetType().Name }
            };
        }

        /// <summary>
        /// Validates the read options.
        /// </summary>
        /// <param name="options">The read options to validate.</param>
        /// <returns>true if the options are valid; otherwise, false.</returns>
        protected override bool ValidateReadOptions(Dictionary<string, object> options)
        {
            return options != null;
        }

        /// <summary>
        /// Validates the write options.
        /// </summary>
        /// <param name="options">The write options to validate.</param>
        /// <returns>true if the options are valid; otherwise, false.</returns>
        protected override bool ValidateWriteOptions(Dictionary<string, object> options)
        {
            if (options == null)
            {
                return false;
            }

            // File path is required for write operations
            return options.ContainsKey("filePath") && !string.IsNullOrWhiteSpace(GetOptionValue<string>(options, "filePath", string.Empty));
        }

        /// <summary>
        /// Finds files matching the pattern.
        /// </summary>
        /// <returns>An array of file paths.</returns>
        private string[] FindFiles()
        {
            var searchOption = _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(_basePath, _filePattern, searchOption);
        }

        /// <summary>
        /// Reads a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A data package containing the file content.</returns>
        private DataPackage ReadFile(string filePath)
        {
            // Combine with base path if not absolute
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(_basePath, filePath);
            }

            // Read file content
            var bytes = File.ReadAllBytes(filePath);

            // Determine content type based on file extension
            var contentType = GetContentTypeFromExtension(Path.GetExtension(filePath));

            // Create source information
            var sourceInfo = new SourceInformation
            {
                SourceId = Path.GetFileName(filePath),
                SourceType = "file",
                SourceLocation = filePath
            };

            // Create data package
            return ProtocolUtilities.CreateDataPackageFromBytes(bytes, contentType, sourceInfo);
        }

        /// <summary>
        /// Gets the content type from a file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The content type.</returns>
        private string GetContentTypeFromExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return "application/octet-stream";
            }

            extension = extension.ToLowerInvariant().TrimStart('.');

            return extension switch
            {
                "txt" => "text/plain",
                "json" => "application/json",
                "xml" => "application/xml",
                "csv" => "text/csv",
                "html" or "htm" => "text/html",
                "pdf" => "application/pdf",
                "zip" => "application/zip",
                "gz" or "gzip" => "application/gzip",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
