namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record TimeSeriesPointDto(
    DateTimeOffset Timestamp,
    long TotalRequests,
    long BlockedRequests);
