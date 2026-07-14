using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Entities;

public sealed class AlertRule : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Condition { get; private set; } = string.Empty;
    public double Threshold { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public bool IsEnabled { get; private set; }
    public int CooldownMinutes { get; private set; }
    public DateTimeOffset? LastTriggeredAt { get; private set; }

    private AlertRule() { }

    public static AlertRule Create(
        string name,
        string condition,
        double threshold,
        AlertSeverity severity,
        int cooldownMinutes,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("AlertRule name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(condition))
            throw new DomainException("AlertRule condition cannot be null or empty.");

        if (threshold < 0)
            throw new DomainException("AlertRule threshold cannot be negative.");

        if (cooldownMinutes < 0)
            throw new DomainException("AlertRule cooldownMinutes cannot be negative.");

        return new AlertRule
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Condition = condition.Trim(),
            Threshold = threshold,
            Severity = severity,
            IsEnabled = true,
            CooldownMinutes = cooldownMinutes,
            LastTriggeredAt = null
        };
    }

    public bool CanTrigger(DateTimeOffset now)
    {
        if (!IsEnabled)
            return false;

        if (LastTriggeredAt is null)
            return true;

        return now >= LastTriggeredAt.Value.AddMinutes(CooldownMinutes);
    }

    public void RecordTrigger(DateTimeOffset triggeredAt)
    {
        LastTriggeredAt = triggeredAt;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
