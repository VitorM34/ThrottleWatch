using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Alerting.Notifiers;

public sealed class EmailNotifier : IAlertNotifier
{
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;

    public EmailNotifier(IOptionsMonitor<ThrottleWatchOptions> options)
    {
        _options = options;
    }

    public string ChannelName => "Email";

    public bool IsEnabled
    {
        get
        {
            var email = _options.CurrentValue.Alerts.Email;
            return email.Enabled
                && !string.IsNullOrWhiteSpace(email.SmtpHost)
                && email.To.Count > 0;
        }
    }

    public async Task NotifyAsync(AlertNotification notification, CancellationToken ct)
    {
        var email = _options.CurrentValue.Alerts.Email;
        var message = BuildMessage(email, notification);

        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = email.UseSsl
                ? SecureSocketOptions.StartTlsWhenAvailable
                : SecureSocketOptions.None;

            await client.ConnectAsync(email.SmtpHost, email.SmtpPort, secureSocketOptions, ct);

            if (!string.IsNullOrWhiteSpace(email.Username))
                await client.AuthenticateAsync(email.Username, email.Password, ct);

            await client.SendAsync(message, ct);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, ct);
        }
    }

    private static MimeMessage BuildMessage(EmailOptions email, AlertNotification notification)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(email.From));

        foreach (var recipient in email.To.Where(r => !string.IsNullOrWhiteSpace(r)))
            message.To.Add(MailboxAddress.Parse(recipient));

        message.Subject = $"[ThrottleWatch] [{notification.Severity}] {notification.RuleName}";
        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>ThrottleWatch Alert</h2>
                <p><strong>Rule:</strong> {System.Net.WebUtility.HtmlEncode(notification.RuleName)}</p>
                <p><strong>Severity:</strong> {notification.Severity}</p>
                <p><strong>Triggered At:</strong> {notification.TriggeredAt:O}</p>
                <p><strong>Message:</strong> {System.Net.WebUtility.HtmlEncode(notification.Message)}</p>
                """
        };

        return message;
    }
}
