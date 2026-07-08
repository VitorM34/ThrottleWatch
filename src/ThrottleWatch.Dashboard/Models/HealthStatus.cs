namespace ThrottleWatch.Dashboard.Models;

public sealed class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public IReadOnlyDictionary<string, HealthEntry> Entries { get; set; } = new Dictionary<string, HealthEntry>();
}

public sealed class HealthEntry
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
    public IReadOnlyDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}
