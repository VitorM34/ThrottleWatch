using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record EndpointMetricsDto(
    string Path,
    string Method,
    long TotalRequests,
    long BlockedRequests,
    long AllowedRequests,
    double AverageLatencyMs,
    string PolicyName,
    string Status,
    DateTimeOffset LastActivity
)
{
    public EndpointMetrics ToModel() => new()
    {
        Path = Path,
        Method = Method,
        TotalRequests = TotalRequests,
        BlockedRequests = BlockedRequests,
        AllowedRequests = AllowedRequests,
        AverageLatencyMs = AverageLatencyMs,
        PolicyName = PolicyName,
        Status = Enum.TryParse<EndpointStatus>(Status, true, out var s) ? s : EndpointStatus.Healthy,
        LastActivity = LastActivity
    };
}
