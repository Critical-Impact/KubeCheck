using KubeCheck.Checks;
using KubeCheck.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KubeCheck.Services;

public class BootService : IHostedService
{
    private readonly Configuration _configuration;
    private readonly ConfigurationService _configurationService;
    private readonly IReadOnlyList<ICheck> _checks;

    public BootService(Configuration configuration, ConfigurationService configurationService, IReadOnlyList<ICheck> checks)
    {
        _configuration = configuration;
        _configurationService = configurationService;
        _checks = checks;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var check in _checks)
        {
            if (!_configuration.CheckIntervals.TryGetValue(check.Name, out var checkInterval))
            {
                _configuration.CheckIntervals[check.Key] = check.CheckInterval.TotalSeconds;
            }
            else
            {
                check.CheckInterval = TimeSpan.FromSeconds(checkInterval);
            }

            if (!_configuration.CheckErrorThresholds.TryGetValue(check.Key, out var checkErrorThreshold))
            {
                _configuration.CheckErrorThresholds[check.Key] = check.ErrorThreshold.TotalSeconds;
            }
            else
            {
                check.ErrorThreshold = TimeSpan.FromSeconds(checkErrorThreshold);
            }
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}