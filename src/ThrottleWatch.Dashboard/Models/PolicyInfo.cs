namespace ThrottleWatch.Dashboard.Models;

public sealed class PolicyInfo
{
    public string Name { get; set; } = string.Empty;
    public int PermitLimit { get; set; }
    public TimeSpan Window { get; set; }
    public string Algorithm { get; set; } = string.Empty;
    public long TotalRequests { get; set; }
    public long RejectedRequests { get; set; }
    public double RejectionRatePercent => TotalRequests > 0
        ? Math.Round((double)RejectedRequests / TotalRequests * 100, 2)
        : 0;
    public bool IsActive { get; set; }
    public int ActiveConnections { get; set; }
}
