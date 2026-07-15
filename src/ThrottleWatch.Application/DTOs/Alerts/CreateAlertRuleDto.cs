using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.DTOs.Alerts;

public sealed record CreateAlertRuleDto(
    string Name,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    int CooldownMinutes,
    string? Description = null);
