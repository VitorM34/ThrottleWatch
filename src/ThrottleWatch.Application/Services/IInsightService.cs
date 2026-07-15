using ThrottleWatch.Application.DTOs.Insights;

namespace ThrottleWatch.Application.Services;

public interface IInsightService
{
    Task<IReadOnlyList<InsightDto>> GetActiveInsightsAsync(CancellationToken ct);
    Task DismissInsightAsync(Guid id, CancellationToken ct);
}
