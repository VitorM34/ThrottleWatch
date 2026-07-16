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

    public async Task<IReadOnlyList<EndpointSummary>> GetTopEndpointsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        return await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from)
            .GroupBy(x => new { x.Path, x.Method })
            .Select(g => new EndpointSummary(
                g.Key.Path,
                g.Key.Method,
                g.LongCount(),
                g.LongCount(x => x.IsBlocked)))
            .OrderByDescending(x => x.RequestCount)
            .Take(top)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClientSummary>> GetTopClientsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        return await _db.MetricEntries
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && (x.ClientIp != null || x.ApiKey != null))
            .GroupBy(x => x.ClientIp ?? x.ApiKey!)
            .Select(g => new ClientSummary(
                g.Key,
                g.LongCount(),
                g.LongCount(x => x.IsBlocked)))
            .OrderByDescending(x => x.RequestCount)
            .Take(top)
            .ToListAsync(ct);
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
