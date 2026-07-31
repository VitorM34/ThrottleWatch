using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record MetricsSummaryApiDto(
    long TotalRequests,
    long TotalBlocked,
    DateTimeOffset From,
    DateTimeOffset To,
    double BlockRatePercent)
{
    public DashboardMetrics ToModel()
    {
        var windowSeconds = Math.Max(1d, (To - From).TotalSeconds);
        return new DashboardMetrics
        {
            TotalRequests = TotalRequests,
            BlockedRequests = TotalBlocked,
            AllowedRequests = Math.Max(0, TotalRequests - TotalBlocked),
            RequestsPerSecond = Math.Round(TotalRequests / windowSeconds, 2),
            AverageLatencyMs = 0,
            ActiveClients = 0,
            Uptime = To - From
        };
    }
}

public sealed record TopEndpointApiDto(
    string Path,
    string Method,
    long RequestCount,
    long BlockedCount)
{
    public EndpointMetrics ToModel()
    {
        var allowed = Math.Max(0, RequestCount - BlockedCount);
        var blockRate = RequestCount == 0 ? 0d : (double)BlockedCount / RequestCount * 100;
        return new EndpointMetrics
        {
            Path = Path,
            Method = Method,
            TotalRequests = RequestCount,
            BlockedRequests = BlockedCount,
            AllowedRequests = allowed,
            AverageLatencyMs = 0,
            PolicyName = string.Empty,
            Status = blockRate >= 50 ? EndpointStatus.Critical
                : blockRate >= 20 ? EndpointStatus.Warning
                : EndpointStatus.Healthy,
            LastActivity = DateTimeOffset.UtcNow
        };
    }
}

public sealed record TopClientApiDto(
    string ClientIdentifier,
    long RequestCount,
    long BlockedCount)
{
    public ClientMetrics ToModel()
    {
        var blockRate = RequestCount == 0 ? 0d : (double)BlockedCount / RequestCount * 100;
        return new ClientMetrics
        {
            IpAddress = ClientIdentifier,
            TotalRequests = RequestCount,
            BlockedRequests = BlockedCount,
            FirstSeen = DateTimeOffset.UtcNow,
            LastSeen = DateTimeOffset.UtcNow,
            RiskLevel = blockRate >= 50 ? ClientRisk.Critical
                : blockRate >= 20 ? ClientRisk.High
                : blockRate >= 5 ? ClientRisk.Medium
                : ClientRisk.Low,
            IsBlocked = BlockedCount > 0 && blockRate >= 80
        };
    }
}

public sealed record TimeSeriesPointApiDto(
    DateTimeOffset Timestamp,
    long TotalRequests,
    long BlockedRequests);
