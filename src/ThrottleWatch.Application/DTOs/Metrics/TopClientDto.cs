namespace ThrottleWatch.Application.DTOs.Metrics;

public sealed record TopClientDto(
    string ClientIdentifier,
    long RequestCount,
    long BlockedCount);
