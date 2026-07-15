using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.DTOs.Alerts;

public sealed record AlertRuleDto(
    Guid Id,
    string Name,
    string? Description,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    bool IsEnabled,
    int CooldownMinutes,
    DateTimeOffset? LastTriggeredAt,
    DateTimeOffset CreatedAt);
