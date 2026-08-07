namespace ThrottleWatch.Infrastructure.Configuration;

public sealed class ThrottleWatchOptions
{
    public const string SectionName = "ThrottleWatch";

    public SecurityOptions Security { get; set; } = new();

    public AlertsOptions Alerts { get; set; } = new();

    public InsightsOptions Insights { get; set; } = new();
}

/// <summary>Shared-secret auth for ThrottleWatch.Api (/api/*).</summary>
public sealed class SecurityOptions
{
    public const string DefaultHeaderName = "X-ThrottleWatch-Key";

    /// <summary>When empty in Development, auth is skipped (with a warning). Required outside Development.</summary>
    public string? ApiKey { get; set; }

    public string HeaderName { get; set; } = DefaultHeaderName;
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
