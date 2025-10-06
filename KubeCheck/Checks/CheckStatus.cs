namespace KubeCheck.Checks;

/// <summary>
/// Indicates the result status of a check.
/// </summary>
public enum CheckStatus
{
    Healthy,
    Warning,
    Error,
    Unknown
}