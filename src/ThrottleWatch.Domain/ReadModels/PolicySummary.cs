namespace ThrottleWatch.Domain.ReadModels;

public sealed record PolicySummary(
    string Name,
    long TotalRequests,
    long BlockedCount);
