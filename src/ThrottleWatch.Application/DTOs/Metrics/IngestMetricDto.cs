namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record IngestMetricDto(
    string Path,
    string Method,
    int StatusCode,
    double DurationMs,
    DateTimeOffset Timestamp,
    string? ClientIp = null,
    string? PolicyName = null,
    string? ApiKey = null);
