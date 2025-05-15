using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Domain.Entities
{
    /// <summary>
    /// Concrete implementation of the AbstractDestinationEntity class for file-based destinations.
    /// Defines a file system location and access protocol for data delivery.
    /// </summary>
    public class FileDestinationEntity : AbstractDestinationEntity
    {
        /// <summary>
        /// Gets or sets the output directory path.
        /// </summary>
        public string OutputDirectoryPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output file name pattern.
        /// </summary>
        public string FileNamePattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file format.
        /// </summary>
        public string FileFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file encoding.
        /// </summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>
        /// Gets or sets the file delimiter for delimited files.
        /// </summary>
        public string? Delimiter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include a header row.
        /// </summary>
        public bool IncludeHeaderRow { get; set; } = true;

        /// <summary>
        /// Gets or sets the file compression type.
        /// </summary>
        public string? CompressionType { get; set; }

        /// <summary>
        /// Gets or sets the file write mode.
        /// </summary>
        public FileWriteMode WriteMode { get; set; } = FileWriteMode.CREATE_NEW;

        /// <summary>
        /// Gets or sets the backup directory path for existing files.
        /// </summary>
        public string? BackupDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the error directory path for failed writes.
        /// </summary>
        public string? ErrorDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the file name pattern for backup files.
        /// </summary>
        public string? BackupFileNamePattern { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create the output directory if it doesn't exist.
        /// </summary>
        public bool CreateDirectoryIfNotExists { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDestinationEntity"/> class.
        /// </summary>
        public FileDestinationEntity()
        {
            Protocol = "file";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDestinationEntity"/> class with the specified destination ID.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        public FileDestinationEntity(string destinationId) : this()
        {
            DestinationId = destinationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDestinationEntity"/> class with the specified destination ID and name.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <param name="name">The name of the destination.</param>
        public FileDestinationEntity(string destinationId, string name) : this(destinationId)
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

            // Validate output directory path
            if (string.IsNullOrWhiteSpace(OutputDirectoryPath))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "OUTPUT_DIRECTORY_REQUIRED", Message = "Output directory path is required." });
            }

            // Validate file name pattern
            if (string.IsNullOrWhiteSpace(FileNamePattern))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "FILE_NAME_PATTERN_REQUIRED", Message = "File name pattern is required." });
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

            // Validate backup directory path if write mode requires it
            if (WriteMode == FileWriteMode.BACKUP_AND_REPLACE && string.IsNullOrWhiteSpace(BackupDirectoryPath))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Code = "BACKUP_DIRECTORY_REQUIRED", Message = "Backup directory path is required for BACKUP_AND_REPLACE write mode." });
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
    /// Represents the file write mode.
    /// </summary>
    public enum FileWriteMode
    {
        /// <summary>
        /// Create a new file, fail if the file already exists.
        /// </summary>
        CREATE_NEW,

        /// <summary>
        /// Create a new file, overwrite if the file already exists.
        /// </summary>
        CREATE_OR_OVERWRITE,

        /// <summary>
        /// Append to the file if it exists, create if it doesn't.
        /// </summary>
        APPEND,

        /// <summary>
        /// Backup the existing file and create a new one.
        /// </summary>
        BACKUP_AND_REPLACE
    }
}
