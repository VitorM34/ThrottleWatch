namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record TopEndpointDto(
    string Path,
    string Method,
    long RequestCount,
    long BlockedCount,
    double AverageLatencyMs,
    string? PolicyName,
    DateTimeOffset LastActivity);
