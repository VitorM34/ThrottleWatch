using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Alerting.Notifiers;

public sealed class SlackNotifier : IAlertNotifier
{
    public const string HttpClientName = "throttlewatch-slack";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<SlackNotifier> _logger;

    public SlackNotifier(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<SlackNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public string ChannelName => "Slack";

    public bool IsEnabled
    {
        get
        {
            var slack = _options.CurrentValue.Alerts.Slack;
            return slack.Enabled && !string.IsNullOrWhiteSpace(slack.WebhookUrl);
        }
    }

    public async Task NotifyAsync(AlertNotification notification, CancellationToken ct)
    {
        var slack = _options.CurrentValue.Alerts.Slack;
        var payload = new
        {
            text = ":warning: *ThrottleWatch Alert*",
            attachments = new[]
            {
                new
                {
                    color = ResolveColor(notification.Severity),
                    fields = new[]
                    {
                        new { title = "Rule", value = notification.RuleName, @short = true },
                        new { title = "Severity", value = notification.Severity.ToString(), @short = true },
                        new { title = "Message", value = notification.Message, @short = false },
                        new { title = "Triggered At", value = notification.TriggeredAt.ToString("O"), @short = true }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsJsonAsync(slack.WebhookUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Slack notifier received HTTP {StatusCode} for rule {RuleName}.",
                (int)response.StatusCode,
                notification.RuleName);
            response.EnsureSuccessStatusCode();
        }
    }

    private static string ResolveColor(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Info => "#0000FF",
        AlertSeverity.Warning => "#FFA500",
        AlertSeverity.Critical => "#FF0000",
        _ => "#808080"
    };
}
