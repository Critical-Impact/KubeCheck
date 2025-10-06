namespace KubeCheck.Checks;

/// <summary>
/// Represents a single cluster check (e.g. pod health, deployment readiness, config validity).
/// </summary>
public interface ICheck
{
    /// <summary>
    /// A name for this check.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// A unique identifier for this check.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The category of the check (optional, e.g. "Networking", "Storage", "Security").
    /// </summary>
    string Category { get; }

    /// <summary>
    /// The interval between checks.
    /// </summary>
    TimeSpan CheckInterval { get; set; }

    /// <summary>
    /// How long before this is considered an error?
    /// </summary>
    TimeSpan ErrorThreshold { get; set; }

    /// <summary>
    /// Executes the check logic and returns the result.
    /// </summary>
    Task<ICheckResult> ExecuteAsync(CancellationToken cancellationToken = default);
}