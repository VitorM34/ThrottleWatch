using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.DTOs.Insights;

public sealed record InsightDto(
    Guid Id,
    InsightType Type,
    string Title,
    string Description,
    AlertSeverity Severity,
    string? AffectedResource,
    DateTimeOffset GeneratedAt,
    bool IsDismissed);
