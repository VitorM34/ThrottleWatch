namespace ThrottleWatch.Client.Http;

public sealed record MetricPayload(
    string Path,
    string Method,
    int StatusCode,
    double DurationMs,
    DateTimeOffset Timestamp,
    string? ClientIp = null,
    string? PolicyName = null,
    string? ApiKey = null);
