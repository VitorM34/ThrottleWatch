using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.DTOs.Insights;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Services;

public sealed class InsightService : IInsightService
{
    private readonly IInsightRepository _repository;
    private readonly ILogger<InsightService> _logger;

    public InsightService(IInsightRepository repository, ILogger<InsightService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InsightDto>> GetActiveInsightsAsync(CancellationToken ct)
    {
        var insights = await _repository.GetActiveInsightsAsync(ct);
        return insights.Select(ToDto).ToList();
    }

    public async Task DismissInsightAsync(Guid id, CancellationToken ct)
    {
        var insight = await _repository.GetByIdAsync(id, ct)
            ?? throw new InsightNotFoundException(id);

        insight.Dismiss();
        await _repository.UpdateAsync(insight, ct);

        _logger.LogInformation("Insight dismissed: {Id}", id);
    }

    private static InsightDto ToDto(Insight insight) => new(
        insight.Id,
        insight.Type,
        insight.Title,
        insight.Description,
        insight.Severity,
        insight.AffectedResource,
        insight.GeneratedAt,
        insight.IsDismissed);
}
