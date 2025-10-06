using KubeCheck.Checks;
using Microsoft.Extensions.Hosting;

namespace KubeCheck.Services;

/// <summary>
/// Background service responsible for executing registered checks periodically.
/// </summary>
public interface ICheckService : IHostedService
{
    /// <summary>
    /// Executes all checks immediately (useful for manual trigger or startup).
    /// </summary>
    Task<IReadOnlyList<ICheckResult>> RunChecksAsync(CancellationToken cancellationToken = default);
}