namespace ThrottleWatch.Dashboard.Models;

public sealed class TimeSeriesPoint
{
    public DateTimeOffset Timestamp { get; set; }
    public double Value { get; set; }
}

public sealed class TimeSeriesData
{
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<TimeSeriesPoint> Points { get; set; } = [];
}
