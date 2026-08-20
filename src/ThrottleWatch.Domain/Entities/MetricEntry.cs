using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Domain.Entities;

public sealed class MetricEntry : Entity
{
    public string TenantId { get; private set; } = TenantIds.Default;
    public string Path { get; private set; } = string.Empty;
    public string Method { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public bool IsBlocked { get; private set; }
    public string? PolicyName { get; private set; }
    public string? ClientIp { get; private set; }
    public string? ApiKey { get; private set; }
    public double DurationMs { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    private MetricEntry() { }

    public static MetricEntry Create(
        string path,
        string method,
        int statusCode,
        double durationMs,
        DateTimeOffset timestamp,
        string? clientIp = null,
        string? policyName = null,
        string? apiKey = null,
        string tenantId = TenantIds.Default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new DomainException("MetricEntry path cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(method))
            throw new DomainException("MetricEntry method cannot be null or empty.");

        if (durationMs < 0)
            throw new DomainException("MetricEntry durationMs cannot be negative.");

        return new MetricEntry
        {
            TenantId = TenantIds.Normalize(tenantId),
            Path = path.Trim().ToLowerInvariant(),
            Method = method.Trim().ToUpperInvariant(),
            StatusCode = statusCode,
            IsBlocked = statusCode == 429,
            PolicyName = policyName,
            ClientIp = clientIp,
            ApiKey = apiKey,
            DurationMs = durationMs,
            Timestamp = timestamp
        };
    }
}
