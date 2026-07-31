namespace ThrottleWatch.Dashboard.Models;

public sealed class InsightInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string? AffectedResource { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
}

public enum InsightType
{
    HighBlockRate = 0,
    SuspiciousClient = 1,
    MisconfiguredPolicy = 2,
    PeakHours = 3,
    UnusualPattern = 4
}
