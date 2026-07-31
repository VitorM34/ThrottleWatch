using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record InsightApiDto(
    Guid Id,
    InsightType Type,
    string Title,
    string Description,
    AlertSeverity Severity,
    string? AffectedResource,
    DateTimeOffset GeneratedAt,
    bool IsDismissed)
{
    public InsightInfo ToModel() => new()
    {
        Id = Id,
        Type = Type,
        Title = Title,
        Description = Description,
        Severity = Severity,
        AffectedResource = AffectedResource,
        GeneratedAt = GeneratedAt
    };
}
