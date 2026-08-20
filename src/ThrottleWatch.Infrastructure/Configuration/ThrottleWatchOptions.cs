namespace ThrottleWatch.Infrastructure.Configuration;

public sealed class ThrottleWatchOptions
{
    public const string SectionName = "ThrottleWatch";

    public SecurityOptions Security { get; set; } = new();

    public StorageOptions Storage { get; set; } = new();

    public AlertsOptions Alerts { get; set; } = new();

    public InsightsOptions Insights { get; set; } = new();
}

/// <summary>Shared-secret auth for ThrottleWatch.Api (/api/*). One API key = one tenant (ADR-013).</summary>
public sealed class SecurityOptions
{
    public const string DefaultHeaderName = "X-ThrottleWatch-Key";

    /// <summary>When empty in Development, auth is skipped (with a warning). Required outside Development unless <see cref="Tenants"/> is set.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Tenant for <see cref="ApiKey"/>. Defaults to <c>default</c> (Compose demo).</summary>
    public string? TenantId { get; set; }

    public string HeaderName { get; set; } = DefaultHeaderName;

    /// <summary>Additional API keys, each mapped to a tenant. Does not replace <see cref="ApiKey"/>.</summary>
    public List<TenantKeyOptions> Tenants { get; set; } = [];
}

public sealed class TenantKeyOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
}

/// <summary>Retention and history rollup settings.</summary>
public sealed class StorageOptions
{
    /// <summary>Days to retain raw metrics and rollups. Default 30.</summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>How often the retention job runs (hours). Default 6.</summary>
    public int RetentionIntervalHours { get; set; } = 6;

    /// <summary>How often the rollup job runs (minutes). Default 1.</summary>
    public int RollupIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// Lookback window (hours) of completed buckets rebuilt each rollup pass.
    /// Idempotent upsert; default 3 hours covers delayed flushes.
    /// </summary>
    public int RollupLookbackHours { get; set; } = 3;

    /// <summary>Clamp invalid/zero config to safe defaults for background workers.</summary>
    public static StorageOptions Normalize(StorageOptions? source)
    {
        source ??= new StorageOptions();
        return new StorageOptions
        {
            RetentionDays = Math.Clamp(source.RetentionDays <= 0 ? 30 : source.RetentionDays, 1, 3650),
            RetentionIntervalHours = Math.Clamp(
                source.RetentionIntervalHours <= 0 ? 6 : source.RetentionIntervalHours, 1, 168),
            RollupIntervalMinutes = Math.Clamp(
                source.RollupIntervalMinutes <= 0 ? 1 : source.RollupIntervalMinutes, 1, 60),
            RollupLookbackHours = Math.Clamp(
                source.RollupLookbackHours <= 0 ? 3 : source.RollupLookbackHours, 1, 168)
        };
    }
}

public sealed class InsightsOptions
{
    public int IntervalMinutes { get; set; } = 5;

    public int DedupWindowMinutes { get; set; } = 60;
}

public sealed class AlertsOptions
{
    public bool Enabled { get; set; } = true;

    public WebhookOptions Webhook { get; set; } = new();

    public SlackOptions Slack { get; set; } = new();

    public DiscordOptions Discord { get; set; } = new();

    public EmailOptions Email { get; set; } = new();
}

public sealed class WebhookOptions
{
    public bool Enabled { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? Secret { get; set; }
}

public sealed class SlackOptions
{
    public bool Enabled { get; set; }

    public string WebhookUrl { get; set; } = string.Empty;
}

public sealed class DiscordOptions
{
    public bool Enabled { get; set; }

    public string WebhookUrl { get; set; } = string.Empty;
}

public sealed class EmailOptions
{
    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string From { get; set; } = "throttlewatch@noreply.com";

    public List<string> To { get; set; } = [];

    public bool UseSsl { get; set; } = true;
}
