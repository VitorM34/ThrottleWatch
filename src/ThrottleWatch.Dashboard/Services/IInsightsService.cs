using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public interface IInsightsService
{
    Task<IReadOnlyList<InsightInfo>> GetActiveInsightsAsync(CancellationToken cancellationToken = default);

    Task<bool> DismissInsightAsync(Guid id, CancellationToken cancellationToken = default);
}
