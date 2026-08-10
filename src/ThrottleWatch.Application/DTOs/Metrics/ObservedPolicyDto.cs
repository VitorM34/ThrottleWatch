namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record ObservedPolicyDto(
    string Name,
    long TotalRequests,
    long BlockedCount)
{
    public double BlockRatePercent =>
        TotalRequests == 0 ? 0d : Math.Round((double)BlockedCount / TotalRequests * 100, 2);
}
