using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record DashboardMetricsDto(
    long TotalRequests,
    long BlockedRequests,
    long AllowedRequests,
    double RequestsPerSecond,
    double AverageLatencyMs,
    int ActiveClients,
    long UptimeSeconds
)
{
    public DashboardMetrics ToModel() => new()
    {
        TotalRequests = TotalRequests,
        BlockedRequests = BlockedRequests,
        AllowedRequests = AllowedRequests,
        RequestsPerSecond = RequestsPerSecond,
        AverageLatencyMs = AverageLatencyMs,
        ActiveClients = ActiveClients,
        Uptime = TimeSpan.FromSeconds(UptimeSeconds)
    };
}
