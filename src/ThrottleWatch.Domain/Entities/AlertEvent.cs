using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Entities;

public sealed class AlertEvent : Entity
{
    public Guid AlertRuleId { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public DateTimeOffset TriggeredAt { get; private set; }
    public bool IsAcknowledged { get; private set; }

    private AlertEvent() { }

    public static AlertEvent Create(
        Guid alertRuleId,
        string ruleName,
        string message,
        AlertSeverity severity)
    {
        if (alertRuleId == Guid.Empty)
            throw new DomainException("AlertEvent alertRuleId cannot be empty.");

        if (string.IsNullOrWhiteSpace(ruleName))
            throw new DomainException("AlertEvent ruleName cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("AlertEvent message cannot be null or empty.");

        return new AlertEvent
        {
            AlertRuleId = alertRuleId,
            RuleName = ruleName.Trim(),
            Message = message.Trim(),
            Severity = severity,
            TriggeredAt = DateTimeOffset.UtcNow,
            IsAcknowledged = false
        };
    }

    public void Acknowledge() => IsAcknowledged = true;
}
