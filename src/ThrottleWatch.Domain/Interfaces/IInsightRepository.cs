using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Domain.Interfaces;

public interface IInsightRepository
{
    Task AddAsync(Insight insight, CancellationToken ct);
    Task<IReadOnlyList<Insight>> GetActiveInsightsAsync(CancellationToken ct);
    Task UpdateAsync(Insight insight, CancellationToken ct);
}
