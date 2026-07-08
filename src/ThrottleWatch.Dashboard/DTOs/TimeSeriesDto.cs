using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.DTOs;

public sealed record TimeSeriesPointDto(DateTimeOffset Timestamp, double Value)
{
    public TimeSeriesPoint ToModel() => new() { Timestamp = Timestamp, Value = Value };
}

public sealed record TimeSeriesDataDto(string Name, IReadOnlyList<TimeSeriesPointDto> Points)
{
    public TimeSeriesData ToModel() => new()
    {
        Name = Name,
        Points = Points.Select(p => p.ToModel()).ToList()
    };
}
