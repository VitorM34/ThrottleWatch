using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Domain.ReadModels;
using ThrottleWatch.Infrastructure.Persistence;
using ThrottleWatch.Infrastructure.Persistence.Models;

namespace ThrottleWatch.Infrastructure.Persistence.Repositories;

public sealed class MetricsRepository : IMetricsRepository
{
    private readonly AppDbContext _db;

    public MetricsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(IEnumerable<MetricEntry> entries, CancellationToken ct)
    {
        await _db.MetricEntries.AddRangeAsync(entries, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<long> GetTotalRequestsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        return _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .LongCountAsync(ct);
    }

    public Task<long> GetTotalBlockedAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        return _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to && x.IsBlocked)
            .LongCountAsync(ct);
    }

    public async Task<double> GetAverageLatencyMsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        var average = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .Select(x => (double?)x.DurationMs)
            .AverageAsync(ct);

        return average is null ? 0d : Math.Round(average.Value, 2);
    }

    public Task<int> GetActiveClientsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        return _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to
                        && (x.ClientIp != null || x.ApiKey != null))
            .Select(x => x.ClientIp ?? x.ApiKey!)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<IReadOnlyList<EndpointSummary>> GetTopEndpointsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var rows = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from)
            .GroupBy(x => new { x.Path, x.Method })
            .Select(g => new
            {
                g.Key.Path,
                g.Key.Method,
                RequestCount = g.Count(),
                BlockedCount = g.Sum(x => x.IsBlocked ? 1 : 0),
                AverageLatencyMs = g.Average(x => x.DurationMs),
                LastActivity = g.Max(x => x.Timestamp)
            })
            .OrderByDescending(x => x.RequestCount)
            .Take(top)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return [];

        var endpointKeys = rows.Select(r => (r.Path, r.Method)).ToHashSet();

        var policyRows = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from
                        && x.PolicyName != null
                        && x.PolicyName != string.Empty)
            .GroupBy(x => new { x.Path, x.Method, x.PolicyName })
            .Select(g => new
            {
                g.Key.Path,
                g.Key.Method,
                PolicyName = g.Key.PolicyName!,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var dominantPolicy = policyRows
            .Where(p => endpointKeys.Contains((p.Path, p.Method)))
            .GroupBy(p => (p.Path, p.Method))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Count)
                    .ThenBy(x => x.PolicyName, StringComparer.Ordinal)
                    .First().PolicyName);

        return rows
            .Select(x => new EndpointSummary(
                x.Path,
                x.Method,
                x.RequestCount,
                x.BlockedCount,
                Math.Round(x.AverageLatencyMs, 2),
                dominantPolicy.GetValueOrDefault((x.Path, x.Method)),
                x.LastActivity))
            .ToList();
    }

    public async Task<IReadOnlyList<ClientSummary>> GetTopClientsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var rows = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && (x.ClientIp != null || x.ApiKey != null))
            .GroupBy(x => x.ClientIp ?? x.ApiKey!)
            .Select(g => new
            {
                ClientIdentifier = g.Key,
                RequestCount = g.Count(),
                BlockedCount = g.Sum(x => x.IsBlocked ? 1 : 0),
                FirstSeen = g.Min(x => x.Timestamp),
                LastSeen = g.Max(x => x.Timestamp)
            })
            .OrderByDescending(x => x.RequestCount)
            .Take(top)
            .ToListAsync(ct);

        return rows
            .Select(x => new ClientSummary(
                x.ClientIdentifier,
                x.RequestCount,
                x.BlockedCount,
                x.FirstSeen,
                x.LastSeen))
            .ToList();
    }

    public async Task<IReadOnlyList<PolicySummary>> GetObservedPoliciesAsync(
        DateTimeOffset from,
        CancellationToken ct)
    {
        var rows = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from
                        && x.PolicyName != null
                        && x.PolicyName != string.Empty)
            .GroupBy(x => x.PolicyName!)
            .Select(g => new
            {
                Name = g.Key,
                TotalRequests = (long)g.Count(),
                BlockedCount = (long)g.Sum(x => x.IsBlocked ? 1 : 0)
            })
            .OrderByDescending(x => x.TotalRequests)
            .ToListAsync(ct);

        return rows
            .Select(x => new PolicySummary(x.Name, x.TotalRequests, x.BlockedCount))
            .ToList();
    }

    public async Task<IReadOnlyList<TimeSeriesPoint>> GetTimeSeriesAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        if (to < from)
            return [];

        // ≤6h → minute resolution; longer windows → hourly (scales with rollups, not raw rows).
        var useMinute = to - from <= TimeSpan.FromHours(6);
        var granularity = useMinute ? RollupGranularity.Minute : RollupGranularity.Hour;

        var fromRollups = await GetTimeSeriesFromRollupsAsync(from, to, granularity, ct);
        if (fromRollups.Count == 0)
            return await AggregateRawInDatabaseAsync(from, to, useMinute, ct);

        // Incomplete coverage (e.g. right after deploy) → SQL aggregate full window.
        var coverageSlack = useMinute ? TimeSpan.FromMinutes(2) : TimeSpan.FromHours(2);
        if (fromRollups[0].Timestamp - from > coverageSlack)
            return await AggregateRawInDatabaseAsync(from, to, useMinute, ct);

        return fromRollups;
    }

    public async Task RebuildRollupsAsync(
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        CancellationToken ct)
    {
        if (toExclusive <= fromInclusive)
            return;

        await UpsertRollupsAsync(fromInclusive, toExclusive, minuteBuckets: true, ct);
        await UpsertRollupsAsync(fromInclusive, toExclusive, minuteBuckets: false, ct);
    }

    public async Task DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        await _db.MetricEntries
            .IgnoreQueryFilters()
            .Where(x => x.Timestamp < cutoff)
            .ExecuteDeleteAsync(ct);

        await _db.MetricRollups
            .IgnoreQueryFilters()
            .Where(x => x.BucketStart < cutoff)
            .ExecuteDeleteAsync(ct);
    }

    private async Task<IReadOnlyList<TimeSeriesPoint>> GetTimeSeriesFromRollupsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        RollupGranularity granularity,
        CancellationToken ct)
    {
        var rows = await _db.MetricRollups
            .AsNoTracking()
            .Where(x => x.Granularity == granularity
                        && x.BucketStart >= from
                        && x.BucketStart <= to)
            .OrderBy(x => x.BucketStart)
            .Select(x => new TimeSeriesPoint(x.BucketStart, x.TotalRequests, x.BlockedRequests))
            .ToListAsync(ct);

        return rows;
    }

    private async Task<IReadOnlyList<TimeSeriesPoint>> AggregateRawInDatabaseAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        bool minuteBuckets,
        CancellationToken ct)
    {
        // Group in SQL (do not materialize raw rows). Date parts translate to PostgreSQL.
        var rows = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .GroupBy(x => new
            {
                x.Timestamp.Year,
                x.Timestamp.Month,
                x.Timestamp.Day,
                x.Timestamp.Hour,
                Minute = minuteBuckets ? x.Timestamp.Minute : 0
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                g.Key.Hour,
                g.Key.Minute,
                TotalRequests = g.LongCount(),
                BlockedRequests = g.Sum(x => x.IsBlocked ? 1L : 0L)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.Day)
            .ThenBy(x => x.Hour)
            .ThenBy(x => x.Minute)
            .ToListAsync(ct);

        return rows
            .Select(x => new TimeSeriesPoint(
                new DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0, TimeSpan.Zero),
                x.TotalRequests,
                x.BlockedRequests))
            .ToList();
    }

    private async Task UpsertRollupsAsync(
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        bool minuteBuckets,
        CancellationToken ct)
    {
        var granularity = minuteBuckets ? RollupGranularity.Minute : RollupGranularity.Hour;

        var aggregated = await _db.MetricEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Timestamp >= fromInclusive && x.Timestamp < toExclusive)
            .GroupBy(x => new
            {
                x.TenantId,
                x.Timestamp.Year,
                x.Timestamp.Month,
                x.Timestamp.Day,
                x.Timestamp.Hour,
                Minute = minuteBuckets ? x.Timestamp.Minute : 0
            })
            .Select(g => new
            {
                g.Key.TenantId,
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                g.Key.Hour,
                g.Key.Minute,
                TotalRequests = g.LongCount(),
                BlockedRequests = g.Sum(x => x.IsBlocked ? 1L : 0L)
            })
            .ToListAsync(ct);

        if (aggregated.Count == 0)
            return;

        var bucketStarts = aggregated
            .Select(x => new DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0, TimeSpan.Zero))
            .Distinct()
            .ToList();

        var tenantIds = aggregated.Select(x => x.TenantId).Distinct().ToList();

        var existing = await _db.MetricRollups
            .IgnoreQueryFilters()
            .Where(x => x.Granularity == granularity
                        && tenantIds.Contains(x.TenantId)
                        && bucketStarts.Contains(x.BucketStart))
            .ToListAsync(ct);

        var byKey = existing.ToDictionary(x => (x.TenantId, x.BucketStart));

        foreach (var row in aggregated)
        {
            var bucketStart = new DateTimeOffset(
                row.Year, row.Month, row.Day, row.Hour, row.Minute, 0, TimeSpan.Zero);

            if (byKey.TryGetValue((row.TenantId, bucketStart), out var rollup))
            {
                rollup.TotalRequests = row.TotalRequests;
                rollup.BlockedRequests = row.BlockedRequests;
            }
            else
            {
                _db.MetricRollups.Add(new MetricRollup
                {
                    Id = Guid.NewGuid(),
                    TenantId = row.TenantId,
                    BucketStart = bucketStart,
                    Granularity = granularity,
                    TotalRequests = row.TotalRequests,
                    BlockedRequests = row.BlockedRequests
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
