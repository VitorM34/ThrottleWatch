using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Persistence;

namespace ThrottleWatch.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;

    public AlertRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AlertRule>> GetAllAsync(CancellationToken ct)
    {
        return await _db.AlertRules
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken ct)
    {
        return await _db.AlertRules
            .Where(x => x.IsEnabled)
            .ToListAsync(ct);
    }

    public Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.AlertRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(AlertRule rule, CancellationToken ct)
    {
        await _db.AlertRules.AddAsync(rule, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AlertRule rule, CancellationToken ct)
    {
        _db.AlertRules.Update(rule);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _db.AlertRules
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task AddEventAsync(AlertEvent alertEvent, CancellationToken ct)
    {
        await _db.AlertEvents.AddAsync(alertEvent, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<AlertEvent?> GetEventByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.AlertEvents.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task UpdateEventAsync(AlertEvent alertEvent, CancellationToken ct)
    {
        _db.AlertEvents.Update(alertEvent);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AlertEvent>> GetRecentEventsAsync(int count, CancellationToken ct)
    {
        return await _db.AlertEvents
            .AsNoTracking()
            .OrderByDescending(x => x.TriggeredAt)
            .Take(count)
            .ToListAsync(ct);
    }
}
