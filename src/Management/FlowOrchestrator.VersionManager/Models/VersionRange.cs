using System.Text.Json.Serialization;

namespace FlowOrchestrator.Management.Versioning.Models
{
    /// <summary>
    /// Represents a version range with minimum and maximum versions.
    /// </summary>
    public class VersionRange
    {
        /// <summary>
        /// Gets or sets the minimum version (inclusive).
        /// </summary>
        [JsonPropertyName("minVersion")]
        public string MinVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum version (inclusive).
        /// </summary>
        [JsonPropertyName("maxVersion")]
        public string MaxVersion { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionRange"/> class.
        /// </summary>
        public VersionRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionRange"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version (inclusive).</param>
        /// <param name="maxVersion">The maximum version (inclusive).</param>
        public VersionRange(string minVersion, string maxVersion)
        {
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        /// <summary>
        /// Determines whether the specified version is within this range.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <returns>True if the version is within the range; otherwise, false.</returns>
        public bool IsVersionInRange(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            // Parse versions into components
            var versionParts = ParseVersion(version);
            var minVersionParts = ParseVersion(MinVersion);
            var maxVersionParts = ParseVersion(MaxVersion);

            // Compare with minimum version (inclusive)
            var minComparison = CompareVersions(versionParts, minVersionParts);
            if (minComparison < 0)
            {
                return false;
            }

            // Compare with maximum version (inclusive)
            var maxComparison = CompareVersions(versionParts, maxVersionParts);
            return maxComparison <= 0;
        }

        private static int[] ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return new int[3];
            }

            var parts = version.Split('.');
            var result = new int[3];

            for (int i = 0; i < Math.Min(parts.Length, 3); i++)
            {
                if (int.TryParse(parts[i], out int value))
                {
                    result[i] = value;
                }
            }

            return result;
        }

        private static int CompareVersions(int[] version1, int[] version2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (version1[i] != version2[i])
                {
                    return version1[i].CompareTo(version2[i]);
                }
            }

            return 0;
        }
    }
}
