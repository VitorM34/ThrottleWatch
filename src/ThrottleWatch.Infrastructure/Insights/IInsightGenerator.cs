using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Insights;

public interface IInsightGenerator
{
    Task<IReadOnlyList<Insight>> GenerateAsync(CancellationToken ct);
}
