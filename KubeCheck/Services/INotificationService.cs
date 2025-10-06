using Discord.Webhook;

namespace KubeCheck.Services;

public interface INotificationService
{
    /// <summary>
    /// Sends a plain text message to the configured notification channel.
    /// </summary>
    Task SendMessageAsync(string message);

    /// <summary>
    /// Sends a rich embed notification with title, description, and optional fields.
    /// </summary>
    Task SendEmbedAsync(string title, string description, string? url = null,
        DColor? color = null,
        IEnumerable<(string name, string value)>? fields = null,
        string? imageUrl = null, string? thumbnailUrl = null,
        string? footerText = null, string? footerIconUrl = null);
}
