namespace ThrottleWatch.Dashboard.Models;

public sealed class EndpointMetrics
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public long TotalRequests { get; set; }
    public long BlockedRequests { get; set; }
    public long AllowedRequests { get; set; }
    public double AverageLatencyMs { get; set; }
    public double BlockRatePercent => TotalRequests > 0
        ? Math.Round((double)BlockedRequests / TotalRequests * 100, 2)
        : 0;
    public string PolicyName { get; set; } = string.Empty;
    public EndpointStatus Status { get; set; }
    public DateTimeOffset LastActivity { get; set; }
}

public enum EndpointStatus
{
    Healthy,
    Warning,
    Critical
}
