using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record AlertInfoDto(
    Guid Id,
    string Title,
    string Message,
    string Severity,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    string? Source
)
{
    public AlertInfo ToModel() => new()
    {
        Id = Id,
        Title = Title,
        Message = Message,
        Severity = Enum.TryParse<AlertSeverity>(Severity, true, out var sev) ? sev : AlertSeverity.Info,
        Status = Enum.TryParse<AlertStatus>(Status, true, out var stat) ? stat : AlertStatus.Active,
        CreatedAt = CreatedAt,
        ResolvedAt = ResolvedAt,
        Source = Source
    };
}
