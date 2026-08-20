using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Infrastructure.Persistence.Models;

/// <summary>Infrastructure projection for pre-aggregated timeseries buckets (not a domain aggregate).</summary>
public sealed class MetricRollup
{
    public Guid Id { get; set; }

    public string TenantId { get; set; } = TenantIds.Default;

    public DateTimeOffset BucketStart { get; set; }

    public RollupGranularity Granularity { get; set; }

    public long TotalRequests { get; set; }

    public long BlockedRequests { get; set; }
}
