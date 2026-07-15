namespace ThrottleWatch.Domain.ReadModels;

public sealed record ClientSummary(
    string ClientIdentifier,
    long RequestCount,
    long BlockedCount);
