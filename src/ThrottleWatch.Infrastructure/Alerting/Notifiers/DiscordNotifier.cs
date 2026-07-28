using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Alerting.Notifiers;

public sealed class DiscordNotifier : IAlertNotifier
{
    public const string HttpClientName = "throttlewatch-discord";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<DiscordNotifier> _logger;

    public DiscordNotifier(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<DiscordNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public string ChannelName => "Discord";

    public bool IsEnabled
    {
        get
        {
            var discord = _options.CurrentValue.Alerts.Discord;
            return discord.Enabled && !string.IsNullOrWhiteSpace(discord.WebhookUrl);
        }
    }

    public async Task NotifyAsync(AlertNotification notification, CancellationToken ct)
    {
        var discord = _options.CurrentValue.Alerts.Discord;
        var payload = new
        {
            content = (string?)null,
            embeds = new[]
            {
                new
                {
                    title = "ThrottleWatch Alert",
                    description = notification.Message,
                    color = ResolveColor(notification.Severity),
                    fields = new[]
                    {
                        new { name = "Rule", value = notification.RuleName, inline = true },
                        new { name = "Severity", value = notification.Severity.ToString(), inline = true },
                        new { name = "Triggered At", value = notification.TriggeredAt.ToString("O"), inline = false }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsJsonAsync(discord.WebhookUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Discord notifier received HTTP {StatusCode} for rule {RuleName}.",
                (int)response.StatusCode,
                notification.RuleName);
            response.EnsureSuccessStatusCode();
        }
    }

    private static int ResolveColor(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Info => 255,
        AlertSeverity.Warning => 16753920,
        AlertSeverity.Critical => 16711680,
        _ => 8421504
    };
}
