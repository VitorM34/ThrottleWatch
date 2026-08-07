using System.ComponentModel.DataAnnotations;

namespace ThrottleWatch.Client.Configuration;

public sealed class ThrottleWatchOptions
{
    public const string SectionName = "ThrottleWatch";

    [Required]
    [Url]
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";

    public bool CaptureOnlyBlocked { get; set; }

    public bool CaptureClientIp { get; set; } = true;

    /// <summary>
    /// Shared secret for authenticating to ThrottleWatch.Api (<c>X-ThrottleWatch-Key</c>).
    /// Not the same as <see cref="ApiKeyHeaderName"/> (which reads keys from inbound app traffic).
    /// </summary>
    public string? ApiKey { get; set; }

    public string? ApiKeyHeaderName { get; set; } = "X-Api-Key";

    public string? PolicyNameHeaderName { get; set; } = "X-RateLimit-Policy";

    [Range(1, 1000)]
    public int BatchSize { get; set; } = 50;

    [Range(100, 60_000)]
    public int FlushIntervalMilliseconds { get; set; } = 1000;

    [Range(100, 100_000)]
    public int BufferCapacity { get; set; } = 10_000;
}
