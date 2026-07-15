namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record MetricsSummaryDto(
    long TotalRequests,
    long TotalBlocked,
    DateTimeOffset From,
    DateTimeOffset To)
{
    public double BlockRatePercent =>
        TotalRequests == 0 ? 0d : Math.Round((double)TotalBlocked / TotalRequests * 100, 2);
}
