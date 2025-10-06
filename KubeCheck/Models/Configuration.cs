using KubeCheck.Checks;

namespace KubeCheck.Models;

public class Configuration
{
    public string? WebhookUrl  { get; set; }
    public string? WebhookAvatarUrl { get; set; }
    public string? WebhookUsername { get; set; }

    public bool NotifyHealthy { get; set; } = true;
    public bool NotifyWarning { get; set; } = true;
    public bool NotifyError { get; set; } = true;
    public bool NotifyUnknown { get; set; } = true;

    public Dictionary<string, double> CheckIntervals { get; set; } = new Dictionary<string, double>();

    public Dictionary<string, double> CheckErrorThresholds { get; set; } = new Dictionary<string, double>();

}