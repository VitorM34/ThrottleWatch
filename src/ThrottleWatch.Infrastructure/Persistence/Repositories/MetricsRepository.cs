using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Domain.ReadModels;
using ThrottleWatch.Infrastructure.Persistence;

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
        var entries = await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .Select(x => new { x.Timestamp, x.IsBlocked })
            .ToListAsync(ct);

        return entries
            .GroupBy(x => new DateTimeOffset(
                x.Timestamp.Year,
                x.Timestamp.Month,
                x.Timestamp.Day,
                x.Timestamp.Hour,
                0,
                0,
                x.Timestamp.Offset))
            .OrderBy(g => g.Key)
            .Select(g => new TimeSeriesPoint(
                g.Key,
                g.LongCount(),
                g.LongCount(x => x.IsBlocked)))
            .ToList();
    }

    public async Task DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        await _db.MetricEntries
            .Where(x => x.Timestamp < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
