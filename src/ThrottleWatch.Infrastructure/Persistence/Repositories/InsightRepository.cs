using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Persistence;

namespace ThrottleWatch.Infrastructure.Persistence.Repositories;

public sealed class InsightRepository : IInsightRepository
{
    private readonly AppDbContext _db;

    public InsightRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Insight insight, CancellationToken ct)
    {
        await _db.Insights.AddAsync(insight, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Insight?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Insights.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Insight>> GetActiveInsightsAsync(CancellationToken ct)
    {
        return await _db.Insights
            .AsNoTracking()
            .Where(x => !x.IsDismissed)
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(Insight insight, CancellationToken ct)
    {
        _db.Insights.Update(insight);
        await _db.SaveChangesAsync(ct);
    }
}
