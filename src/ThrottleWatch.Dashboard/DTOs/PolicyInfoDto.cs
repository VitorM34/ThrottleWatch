using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record PolicyInfoDto(
    string Name,
    int PermitLimit,
    int WindowSeconds,
    string Algorithm,
    long TotalRequests,
    long RejectedRequests,
    bool IsActive,
    int ActiveConnections
)
{
    public PolicyInfo ToModel() => new()
    {
        Name = Name,
        PermitLimit = PermitLimit,
        Window = TimeSpan.FromSeconds(WindowSeconds),
        Algorithm = Algorithm,
        TotalRequests = TotalRequests,
        RejectedRequests = RejectedRequests,
        IsActive = IsActive,
        ActiveConnections = ActiveConnections
    };
}
