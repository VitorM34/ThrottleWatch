using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.DTOs.Alerts;

public sealed record AlertEventDto(
    Guid Id,
    Guid AlertRuleId,
    string RuleName,
    string Message,
    AlertSeverity Severity,
    DateTimeOffset TriggeredAt,
    bool IsAcknowledged);
