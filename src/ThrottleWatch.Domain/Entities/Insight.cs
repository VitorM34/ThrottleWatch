using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Domain.Entities;

public sealed class Insight : Entity
{
    public string TenantId { get; private set; } = TenantIds.Default;
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
        string? affectedResource = null,
        string tenantId = TenantIds.Default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Insight title cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Insight description cannot be null or empty.");

        return new Insight
        {
            TenantId = TenantIds.Normalize(tenantId),
            Type = type,
            Title = title.Trim(),
            Description = description.Trim(),
            Severity = severity,
            AffectedResource = affectedResource?.Trim(),
            GeneratedAt = DateTimeOffset.UtcNow,
            IsDismissed = false
        };
    }

    public void AssignTenant(string tenantId) => TenantId = TenantIds.Normalize(tenantId);

    public void Dismiss() => IsDismissed = true;
}
