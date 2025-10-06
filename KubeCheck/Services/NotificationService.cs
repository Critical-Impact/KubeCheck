using Discord.Webhook;
using KubeCheck.Models;
using Microsoft.Extensions.Logging;

namespace KubeCheck.Services;

/// <summary>
/// Implementation of INotificationService using Discord webhooks.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly Configuration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(Configuration configuration, ILogger<NotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        var webhook = CreateWebhook();
        var webhookObject = new WebhookObject
        {
            content = message
        };

        await webhook.SendAsync(webhookObject);
    }

    /// <inheritdoc />
    public async Task SendEmbedAsync(string title, string description, string? url = null,
        DColor? color = null,
        IEnumerable<(string name, string value)>? fields = null,
        string? imageUrl = null, string? thumbnailUrl = null,
        string? footerText = null, string? footerIconUrl = null)
    {
        var webhookObject = new WebhookObject();

        webhookObject.AddEmbed(builder =>
        {
            builder.WithTitle(title)
                .WithDescription(description);
            if (url != null)
            {
                builder.WithUrl(url);
            }

            if (!string.IsNullOrEmpty(thumbnailUrl))
                builder.WithThumbnail(thumbnailUrl);

            if (!string.IsNullOrEmpty(imageUrl))
                builder.WithImage(imageUrl);

            if (color != null)
                builder.WithColor(color);

            if (!string.IsNullOrEmpty(footerText) && footerIconUrl != null)
                builder.WithFooter(footerText, footerIconUrl);

            if (fields != null)
            {
                foreach (var (name, value) in fields)
                    builder.AddField(name, value);
            }
        });

        if (IsConfigured())
        {
            var webhook = CreateWebhook();
            await webhook.SendAsync(webhookObject);
        }
        else
        {
            _logger.LogError("Webhook not configured.");
        }
    }

    private bool IsConfigured()
    {
        return _configuration.WebhookUrl != null;
    }

    private Webhook CreateWebhook()
    {
        if (string.IsNullOrWhiteSpace(_configuration.WebhookUrl))
            throw new ArgumentException("Webhook url is not provided.", nameof(_configuration.WebhookUrl));
        return new Webhook(_configuration.WebhookUrl, _configuration.WebhookUsername, _configuration.WebhookAvatarUrl);
    }
}
