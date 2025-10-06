namespace KubeCheck.Checks;

/// <inheritdoc />
public class CheckResult : ICheckResult
{
    /// <inheritdoc />
    public string CheckName { get; set; }
    /// <inheritdoc />
    public CheckStatus Status { get; set; }
    /// <inheritdoc />
    public string Message { get; set; }
    /// <inheritdoc />
    public IReadOnlyDictionary<string, object>? Metadata { get; set; }
    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; set; }

    public CheckResult(string checkName, CheckStatus status, string message, IReadOnlyDictionary<string, object>? metadata, DateTimeOffset timestamp)
    {
        CheckName = checkName;
        Status = status;
        Message = message;
        Metadata = metadata;
        Timestamp = timestamp;
    }
}