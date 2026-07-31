using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Domain.Interfaces;

public interface IInsightRepository
{
    Task AddAsync(Insight insight, CancellationToken ct);
    Task<Insight?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Insight>> GetActiveInsightsAsync(CancellationToken ct);
    Task UpdateAsync(Insight insight, CancellationToken ct);
    Task<bool> ExistsRecentAsync(
        InsightType type,
        string? affectedResource,
        TimeSpan window,
        CancellationToken ct);
}
