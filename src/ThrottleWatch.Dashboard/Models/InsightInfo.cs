namespace ThrottleWatch.Dashboard.Models;

public sealed class InsightInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public InsightPriority Priority { get; set; }
    public string? Recommendation { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public enum InsightType
{
    AnomalyDetected,
    TrafficSpike,
    SuspiciousClient,
    PolicyOptimization,
    CapacityWarning,
    PatternDetected
}

public enum InsightPriority
{
    Low,
    Medium,
    High
}
