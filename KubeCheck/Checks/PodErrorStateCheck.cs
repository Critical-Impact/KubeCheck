using k8s;
using k8s.Models;

namespace KubeCheck.Checks;

public class PodErrorStateCheck : ICheck
{
    private readonly IKubernetes _client;

    public string Name => "Pod Error State Check";
    public string Key => "PodErrorStateCheck";
    public string Category => "Workload Health";
    public TimeSpan CheckInterval { get; set; } =  TimeSpan.FromMinutes(60);
    public TimeSpan ErrorThreshold { get; set; } =  TimeSpan.FromMinutes(10);

    public PodErrorStateCheck(IKubernetes client, TimeSpan? errorThreshold = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<ICheckResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var failingPods = new List<V1Pod>();

        var pods = await _client.CoreV1.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var pod in pods.Items)
        {
            if (pod.Status == null || pod.Status.ContainerStatuses == null)
                continue;

            foreach (var status in pod.Status.ContainerStatuses)
            {
                if (status.State?.Waiting?.Reason is { } reason)
                {
                    if (IsErrorState(reason))
                    {
                        var lastTransition = GetLastTransitionTime(pod);
                        if (lastTransition.HasValue && now - lastTransition.Value >= ErrorThreshold)
                        {
                            failingPods.Add(pod);
                            break;
                        }
                    }
                }
            }
        }

        if (failingPods.Count == 0)
        {
            return new CheckResult(Name, CheckStatus.Healthy, $"No pods have been in an error state for more than {ErrorThreshold.TotalMinutes} minutes.", null, DateTimeOffset.Now);
        }

        var description = $"Detected {failingPods.Count} pods in persistent error states:";
        var details = string.Join("\n", failingPods.Select(p =>
            $"- {p.Metadata?.NamespaceProperty}/{p.Metadata?.Name}: {p.Status?.ContainerStatuses?.FirstOrDefault()?.State?.Waiting?.Reason}"));

        return new CheckResult(Name, CheckStatus.Error, $"{description}\n{details}",
            new Dictionary<string, object>
            {
                ["podCount"] = failingPods.Count,
                ["thresholdMinutes"] = ErrorThreshold.TotalMinutes,
                ["pods"] = failingPods.Select(p => new
                {
                    Namespace = p.Metadata?.NamespaceProperty,
                    Name = p.Metadata?.Name,
                    Reason = p.Status?.ContainerStatuses?.FirstOrDefault()?.State?.Waiting?.Reason,
                    Message = p.Status?.ContainerStatuses?.FirstOrDefault()?.State?.Waiting?.Message
                }).ToList()
            }, DateTimeOffset.Now);
    }

    private static bool IsErrorState(string reason)
    {
        var errorStates = new[]
        {
            "Error",
            "CrashLoopBackOff",
            "ImagePullBackOff",
            "RunContainerError",
            "CreateContainerConfigError",
            "CreateContainerError",
            "InvalidImageName",
            "ErrImagePull",
        };

        return errorStates.Contains(reason, StringComparer.OrdinalIgnoreCase);
    }

    private static DateTime? GetLastTransitionTime(V1Pod pod)
    {
        return pod.Status?.Conditions?
            .OrderByDescending(c => c.LastTransitionTime)
            .FirstOrDefault()?.LastTransitionTime?.ToUniversalTime();
    }
}