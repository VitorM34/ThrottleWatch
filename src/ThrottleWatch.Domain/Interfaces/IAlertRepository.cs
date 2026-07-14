using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Domain.Interfaces;

public interface IAlertRepository
{
    Task<IReadOnlyList<AlertRule>> GetActiveRulesAsync(CancellationToken ct);
    Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(AlertRule rule, CancellationToken ct);
    Task UpdateAsync(AlertRule rule, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task AddEventAsync(AlertEvent alertEvent, CancellationToken ct);
    Task<IReadOnlyList<AlertEvent>> GetRecentEventsAsync(int count, CancellationToken ct);
}
