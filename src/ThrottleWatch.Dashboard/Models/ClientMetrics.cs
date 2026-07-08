namespace ThrottleWatch.Dashboard.Models;

public sealed class ClientMetrics
{
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public long TotalRequests { get; set; }
    public long BlockedRequests { get; set; }
    public double BlockRatePercent => TotalRequests > 0
        ? Math.Round((double)BlockedRequests / TotalRequests * 100, 2)
        : 0;
    public DateTimeOffset FirstSeen { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public ClientRisk RiskLevel { get; set; }
    public bool IsBlocked { get; set; }
}

public enum ClientRisk
{
    Low,
    Medium,
    High,
    Critical
}
