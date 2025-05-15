using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractSourceEntity class for file-based sources.
    /// Defines a file system location and access protocol for data retrieval.
    /// </summary>
    public class FileSourceEntity : AbstractSourceEntity
    {
        /// <summary>
        /// Gets or sets the file path pattern for the source.
        /// </summary>
        public string FilePathPattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file format.
        /// </summary>
        public string FileFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to process files recursively in subdirectories.
        /// </summary>
        public bool RecursiveSearch { get; set; } = false;

        /// <summary>
        /// Gets or sets the file encoding.
        /// </summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>
        /// Gets or sets the file delimiter for delimited files.
        /// </summary>
        public string? Delimiter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has a header row.
        /// </summary>
        public bool HasHeaderRow { get; set; } = false;

        /// <summary>
        /// Gets or sets the file compression type.
        /// </summary>
        public string? CompressionType { get; set; }

        /// <summary>
        /// Gets or sets the file processing mode.
        /// </summary>
        public FileProcessingMode ProcessingMode { get; set; } = FileProcessingMode.PROCESS_AND_MOVE;

        /// <summary>
        /// Gets or sets the archive directory path for processed files.
        /// </summary>
        public string? ArchiveDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the error directory path for failed files.
        /// </summary>
        public string? ErrorDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the file name pattern for archived files.
        /// </summary>
        public string? ArchiveFileNamePattern { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSourceEntity"/> class.
        /// </summary>
        public FileSourceEntity()
        {
            Protocol = "file";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSourceEntity"/> class with the specified source ID.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        public FileSourceEntity(string sourceId) : this()
        {
            SourceId = sourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSourceEntity"/> class with the specified source ID and name.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="name">The name of the source.</param>
        public FileSourceEntity(string sourceId, string name) : this(sourceId)
        {
            Name = name;
        }

        /// <summary>
        /// Validates the protocol-specific configuration.
        /// </summary>
        /// <returns>The validation result.</returns>
        protected override ValidationResult ValidateProtocolConfiguration()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate file path pattern
            if (string.IsNullOrWhiteSpace(FilePathPattern))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FILE_PATH_PATTERN_REQUIRED", Message = "File path pattern is required." });
            }

            // Validate file format
            if (string.IsNullOrWhiteSpace(FileFormat))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FILE_FORMAT_REQUIRED", Message = "File format is required." });
            }

            // Validate delimiter for delimited files
            if (FileFormat.Equals("csv", StringComparison.OrdinalIgnoreCase) || 
                FileFormat.Equals("tsv", StringComparison.OrdinalIgnoreCase) || 
                FileFormat.Equals("delimited", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(Delimiter))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "DELIMITER_REQUIRED", Message = "Delimiter is required for delimited file formats." });
                }
            }

            // Validate archive directory path if processing mode requires it
            if (ProcessingMode == FileProcessingMode.PROCESS_AND_MOVE || 
                ProcessingMode == FileProcessingMode.PROCESS_AND_ARCHIVE)
            {
                if (string.IsNullOrWhiteSpace(ArchiveDirectoryPath))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "ARCHIVE_DIRECTORY_REQUIRED", Message = "Archive directory path is required for the selected processing mode." });
                }
            }

            // Validate error directory path
            if (string.IsNullOrWhiteSpace(ErrorDirectoryPath))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "ERROR_DIRECTORY_REQUIRED", Message = "Error directory path is required." });
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the file processing mode.
    /// </summary>
    public enum FileProcessingMode
    {
        /// <summary>
        /// Process the file and leave it in place.
        /// </summary>
        PROCESS_ONLY,

        /// <summary>
        /// Process the file and move it to the archive directory.
        /// </summary>
        PROCESS_AND_MOVE,

        /// <summary>
        /// Process the file and copy it to the archive directory.
        /// </summary>
        PROCESS_AND_ARCHIVE,

        /// <summary>
        /// Process the file and delete it.
        /// </summary>
        PROCESS_AND_DELETE
    }
}
