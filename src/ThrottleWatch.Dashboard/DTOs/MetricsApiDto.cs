using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record MetricsSummaryApiDto(
    long TotalRequests,
    long TotalBlocked,
    DateTimeOffset From,
    DateTimeOffset To,
    double BlockRatePercent,
    double AverageLatencyMs,
    int ActiveClients)
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
            AverageLatencyMs = AverageLatencyMs,
            ActiveClients = ActiveClients,
            Uptime = To - From
        };
    }
}

public sealed record TopEndpointApiDto(
    string Path,
    string Method,
    long RequestCount,
    long BlockedCount,
    double AverageLatencyMs,
    string? PolicyName,
    DateTimeOffset LastActivity)
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
            AverageLatencyMs = AverageLatencyMs,
            PolicyName = PolicyName ?? string.Empty,
            Status = blockRate >= 50 ? EndpointStatus.Critical
                : blockRate >= 20 ? EndpointStatus.Warning
                : EndpointStatus.Healthy,
            LastActivity = LastActivity
        };
    }
}

public sealed record TopClientApiDto(
    string ClientIdentifier,
    long RequestCount,
    long BlockedCount,
    DateTimeOffset FirstSeen,
    DateTimeOffset LastSeen)
{
    public ClientMetrics ToModel()
    {
        var blockRate = RequestCount == 0 ? 0d : (double)BlockedCount / RequestCount * 100;
        return new ClientMetrics
        {
            IpAddress = ClientIdentifier,
            TotalRequests = RequestCount,
            BlockedRequests = BlockedCount,
            FirstSeen = FirstSeen,
            LastSeen = LastSeen,
            RiskLevel = blockRate >= 50 ? ClientRisk.Critical
                : blockRate >= 20 ? ClientRisk.High
                : blockRate >= 5 ? ClientRisk.Medium
                : ClientRisk.Low,
            IsBlocked = BlockedCount > 0 && blockRate >= 80
        };
    }
}

public sealed record ObservedPolicyApiDto(
    string Name,
    long TotalRequests,
    long BlockedCount)
{
    public PolicyInfo ToModel() => new()
    {
        Name = Name,
        TotalRequests = TotalRequests,
        RejectedRequests = BlockedCount,
        // Not available from ingested metrics — UI shows "—" for these.
        PermitLimit = 0,
        Window = TimeSpan.Zero,
        Algorithm = string.Empty,
        IsActive = true,
        ActiveConnections = 0
    };
}

public sealed record TimeSeriesPointApiDto(
    DateTimeOffset Timestamp,
    long TotalRequests,
    long BlockedRequests);
