namespace ThrottleWatch.Domain.Enums;

/// <summary>Pre-aggregated metric bucket size used by history/timeseries.</summary>
public enum RollupGranularity : byte
{
    Minute = 0,
    Hour = 1
}
