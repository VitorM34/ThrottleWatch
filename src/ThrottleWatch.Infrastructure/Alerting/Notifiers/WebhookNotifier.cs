using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Alerting.Notifiers;

public sealed class WebhookNotifier : IAlertNotifier
{
    public const string HttpClientName = "throttlewatch-webhook";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<WebhookNotifier> _logger;

    public WebhookNotifier(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<WebhookNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public string ChannelName => "Webhook";

    public bool IsEnabled
    {
        get
        {
            var webhook = _options.CurrentValue.Alerts.Webhook;
            return webhook.Enabled && !string.IsNullOrWhiteSpace(webhook.Url);
        }
    }

    public async Task NotifyAsync(AlertNotification notification, CancellationToken ct)
    {
        var webhook = _options.CurrentValue.Alerts.Webhook;
        var payload = new
        {
            ruleName = notification.RuleName,
            severity = notification.Severity.ToString(),
            message = notification.Message,
            triggeredAt = notification.TriggeredAt,
            affectedResource = notification.AffectedResource,
            source = "ThrottleWatch"
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(webhook.Secret))
        {
            var signature = ComputeSignature(json, webhook.Secret);
            request.Headers.TryAddWithoutValidation("X-ThrottleWatch-Signature", signature);
        }

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Webhook notifier received HTTP {StatusCode} for rule {RuleName}.",
                (int)response.StatusCode,
                notification.RuleName);
            response.EnsureSuccessStatusCode();
        }
    }

    private static string ComputeSignature(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
