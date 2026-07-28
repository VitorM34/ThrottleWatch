using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Infrastructure.Alerting;

public sealed record AlertNotification(
    string RuleName,
    string Message,
    AlertSeverity Severity,
    DateTimeOffset TriggeredAt,
    string? AffectedResource = null);
