namespace KubeCheck.Checks;

/// <summary>
/// Represents the result of a single check execution.
/// </summary>
public interface ICheckResult
{
    /// <summary>
    /// The check name this result came from.
    /// </summary>
    string CheckName { get; }
    
    /// <summary>
    /// The status of the check.
    /// </summary>
    CheckStatus Status { get; }
    
    /// <summary>
    /// A message describing the result, error, or summary.
    /// </summary>
    string Message { get; }
    
    /// <summary>
    /// Optional: any extra metadata or structured output.
    /// </summary>
    IReadOnlyDictionary<string, object>? Metadata { get; }
    
    /// <summary>
    /// Timestamp of when the check was executed.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}