using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record ClientMetricsDto(
    string IpAddress,
    string? UserAgent,
    long TotalRequests,
    long BlockedRequests,
    DateTimeOffset FirstSeen,
    DateTimeOffset LastSeen,
    string RiskLevel,
    bool IsBlocked
)
{
    public ClientMetrics ToModel() => new()
    {
        IpAddress = IpAddress,
        UserAgent = UserAgent,
        TotalRequests = TotalRequests,
        BlockedRequests = BlockedRequests,
        FirstSeen = FirstSeen,
        LastSeen = LastSeen,
        RiskLevel = Enum.TryParse<ClientRisk>(RiskLevel, true, out var r) ? r : ClientRisk.Low,
        IsBlocked = IsBlocked
    };
}
