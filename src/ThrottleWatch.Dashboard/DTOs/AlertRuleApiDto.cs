using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record AlertRuleApiDto(
    Guid Id,
    string Name,
    string? Description,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    bool IsEnabled,
    int CooldownMinutes,
    DateTimeOffset? LastTriggeredAt,
    DateTimeOffset CreatedAt)
{
    public AlertRuleInfo ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Condition = Condition,
        Threshold = Threshold,
        Severity = Severity,
        IsEnabled = IsEnabled,
        CooldownMinutes = CooldownMinutes,
        LastTriggeredAt = LastTriggeredAt,
        CreatedAt = CreatedAt
    };
}

public sealed record AlertEventApiDto(
    Guid Id,
    Guid AlertRuleId,
    string RuleName,
    string Message,
    AlertSeverity Severity,
    DateTimeOffset TriggeredAt,
    bool IsAcknowledged)
{
    public AlertInfo ToModel() => new()
    {
        Id = Id,
        Title = RuleName,
        Message = Message,
        Severity = Severity,
        Status = IsAcknowledged ? AlertStatus.Acknowledged : AlertStatus.Active,
        CreatedAt = TriggeredAt,
        Source = AlertRuleId.ToString("D")
    };
}

public sealed record CreateAlertRuleRequest(
    string Name,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    int CooldownMinutes,
    string? Description);

public sealed record UpdateAlertRuleRequest(
    string Name,
    string Condition,
    double Threshold,
    AlertSeverity Severity,
    int CooldownMinutes,
    bool IsEnabled,
    string? Description);
