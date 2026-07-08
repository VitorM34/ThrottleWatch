namespace ThrottleWatch.Dashboard.Models;

public sealed class DashboardMetrics
{
    public long TotalRequests { get; set; }
    public long BlockedRequests { get; set; }
    public long AllowedRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageLatencyMs { get; set; }
    public int ActiveClients { get; set; }
    public TimeSpan Uptime { get; set; }
    public double BlockRatePercent => TotalRequests > 0
        ? Math.Round((double)BlockedRequests / TotalRequests * 100, 2)
        : 0;
}
