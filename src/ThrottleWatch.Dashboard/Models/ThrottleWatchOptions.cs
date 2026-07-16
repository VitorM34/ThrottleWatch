using System.ComponentModel.DataAnnotations;

namespace ThrottleWatch.Dashboard.Models;

public sealed class ThrottleWatchOptions
{
    public const string SectionName = "ThrottleWatch";

    [Required]
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";

    [Range(1, 60)]
    public int RefreshIntervalSeconds { get; set; } = 5;

    [Range(10, 1000)]
    public int MaxDataPoints { get; set; } = 60;
}
