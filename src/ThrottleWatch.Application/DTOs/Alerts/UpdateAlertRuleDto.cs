using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.DTOs.Alerts;

public sealed record UpdateAlertRuleDto(
    string Name,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    int CooldownMinutes,
    bool IsEnabled,
    string? Description = null);
