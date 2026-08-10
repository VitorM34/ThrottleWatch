namespace ThrottleWatch.Domain.ReadModels;

public sealed record EndpointSummary(
    string Path,
    string Method,
    long RequestCount,
    long BlockedCount,
    double AverageLatencyMs,
    string? PolicyName,
    DateTimeOffset LastActivity);
