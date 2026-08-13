using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.ReadModels;

namespace ThrottleWatch.Domain.Interfaces;

public interface IMetricsRepository
{
    Task AddRangeAsync(IEnumerable<MetricEntry> entries, CancellationToken ct);
    Task<long> GetTotalRequestsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<long> GetTotalBlockedAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<double> GetAverageLatencyMsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<int> GetActiveClientsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IReadOnlyList<EndpointSummary>> GetTopEndpointsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<ClientSummary>> GetTopClientsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<PolicySummary>> GetObservedPoliciesAsync(DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<TimeSeriesPoint>> GetTimeSeriesAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    /// <summary>Aggregates raw metrics into minute/hour rollup buckets for [from, toExclusive).</summary>
    Task RebuildRollupsAsync(DateTimeOffset fromInclusive, DateTimeOffset toExclusive, CancellationToken ct);
    /// <summary>Deletes raw metrics and rollups with timestamp/bucket older than cutoff.</summary>
    Task DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct);
}
