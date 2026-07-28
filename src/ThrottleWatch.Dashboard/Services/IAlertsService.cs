using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public interface IAlertsService
{
    Task<IReadOnlyList<AlertRuleInfo>> GetRulesAsync(CancellationToken cancellationToken = default);

    Task<AlertRuleInfo?> CreateRuleAsync(AlertRuleFormModel model, CancellationToken cancellationToken = default);

    Task<AlertRuleInfo?> UpdateRuleAsync(AlertRuleFormModel model, CancellationToken cancellationToken = default);

    Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertInfo>> GetEventsAsync(int count = 50, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgeEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
