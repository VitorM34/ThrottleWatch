namespace ThrottleWatch.Domain.ReadModels;

public sealed record TimeSeriesPoint(
    DateTimeOffset Timestamp,
    long TotalRequests,
    long BlockedRequests);
