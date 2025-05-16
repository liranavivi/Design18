using System;
using System.Collections.Generic;

namespace FlowOrchestrator.Observability.Analytics.Models
{
    /// <summary>
    /// Represents an optimization recommendation result.
    /// </summary>
    public class OptimizationRecommendationResult
    {
        /// <summary>
        /// Gets or sets the result identifier.
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the timestamp when the recommendations were generated.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the time range used for analysis.
        /// </summary>
        public TimeRange TimeRange { get; set; } = new TimeRange();

        /// <summary>
        /// Gets or sets the list of recommendations.
        /// </summary>
        public List<OptimizationRecommendation> Recommendations { get; set; } = new List<OptimizationRecommendation>();

        /// <summary>
        /// Gets or sets the summary of recommendations by category.
        /// </summary>
        public Dictionary<string, int> RecommendationSummary { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Represents an optimization recommendation.
    /// </summary>
    public class OptimizationRecommendation
    {
        /// <summary>
        /// Gets or sets the recommendation identifier.
        /// </summary>
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the recommendation category.
        /// </summary>
        public RecommendationCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the recommendation title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recommendation description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recommendation priority.
        /// </summary>
        public RecommendationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0).
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the estimated impact.
        /// </summary>
        public RecommendationImpact Impact { get; set; } = new RecommendationImpact();

        /// <summary>
        /// Gets or sets the implementation steps.
        /// </summary>
        public List<string> ImplementationSteps { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the target entity.
        /// </summary>
        public TargetEntity Target { get; set; } = new TargetEntity();

        /// <summary>
        /// Gets or sets the supporting evidence.
        /// </summary>
        public List<SupportingEvidence> Evidence { get; set; } = new List<SupportingEvidence>();
    }

    /// <summary>
    /// Represents the impact of a recommendation.
    /// </summary>
    public class RecommendationImpact
    {
        /// <summary>
        /// Gets or sets the performance improvement percentage.
        /// </summary>
        public double PerformanceImprovementPercentage { get; set; }

        /// <summary>
        /// Gets or sets the resource savings percentage.
        /// </summary>
        public double ResourceSavingsPercentage { get; set; }

        /// <summary>
        /// Gets or sets the reliability improvement percentage.
        /// </summary>
        public double ReliabilityImprovementPercentage { get; set; }

        /// <summary>
        /// Gets or sets the implementation complexity.
        /// </summary>
        public ImplementationComplexity ImplementationComplexity { get; set; }

        /// <summary>
        /// Gets or sets the impact description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a target entity for a recommendation.
    /// </summary>
    public class TargetEntity
    {
        /// <summary>
        /// Gets or sets the entity type.
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents supporting evidence for a recommendation.
    /// </summary>
    public class SupportingEvidence
    {
        /// <summary>
        /// Gets or sets the evidence type.
        /// </summary>
        public string EvidenceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the evidence description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the evidence data.
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Enum for recommendation category.
    /// </summary>
    public enum RecommendationCategory
    {
        /// <summary>
        /// Resource allocation recommendation.
        /// </summary>
        ResourceAllocation,

        /// <summary>
        /// Flow structure recommendation.
        /// </summary>
        FlowStructure,

        /// <summary>
        /// Branch parallelism recommendation.
        /// </summary>
        BranchParallelism,

        /// <summary>
        /// Component selection recommendation.
        /// </summary>
        ComponentSelection,

        /// <summary>
        /// Merge strategy recommendation.
        /// </summary>
        MergeStrategy,

        /// <summary>
        /// Memory management recommendation.
        /// </summary>
        MemoryManagement,

        /// <summary>
        /// Error handling recommendation.
        /// </summary>
        ErrorHandling
    }

    /// <summary>
    /// Enum for recommendation priority.
    /// </summary>
    public enum RecommendationPriority
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        Low,

        /// <summary>
        /// Medium priority.
        /// </summary>
        Medium,

        /// <summary>
        /// High priority.
        /// </summary>
        High,

        /// <summary>
        /// Critical priority.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Enum for implementation complexity.
    /// </summary>
    public enum ImplementationComplexity
    {
        /// <summary>
        /// Low complexity.
        /// </summary>
        Low,

        /// <summary>
        /// Medium complexity.
        /// </summary>
        Medium,

        /// <summary>
        /// High complexity.
        /// </summary>
        High
    }
}
