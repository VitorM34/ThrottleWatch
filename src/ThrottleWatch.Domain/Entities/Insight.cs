using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Entities;

public sealed class Insight : Entity
{
    public InsightType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public string? AffectedResource { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public bool IsDismissed { get; private set; }

    private Insight() { }

    public static Insight Create(
        InsightType type,
        string title,
        string description,
        AlertSeverity severity,
        string? affectedResource = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Insight title cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Insight description cannot be null or empty.");

        return new Insight
        {
            Type = type,
            Title = title.Trim(),
            Description = description.Trim(),
            Severity = severity,
            AffectedResource = affectedResource?.Trim(),
            GeneratedAt = DateTimeOffset.UtcNow,
            IsDismissed = false
        };
    }

    public void Dismiss() => IsDismissed = true;
}
