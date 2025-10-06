using Discord.Webhook;
using Humanizer;
using KubeCheck.Checks;
using KubeCheck.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KubeCheck.Services;

public class CheckService : BackgroundService, ICheckService
{
    private readonly INotificationService _notificationService;
    private readonly Configuration _configuration;
    private IReadOnlyList<ICheck> _checks;
    private readonly ILogger<CheckService> _logger;
    private readonly Dictionary<ICheck, DateTime> _nextRunTimes;

    public CheckService(INotificationService notificationService, Configuration configuration, IReadOnlyList<ICheck> checks, ILogger<CheckService> logger)
    {
        _notificationService = notificationService;
        _configuration = configuration;
        _checks = checks;
        _logger = logger;
        _nextRunTimes = new();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var results = await RunChecksAsync(stoppingToken);
            foreach (var result in results)
            {
                await SendNotification(result);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SendNotification(ICheckResult checkResult)
    {
        switch (checkResult.Status)
        {
            case CheckStatus.Healthy when _configuration.NotifyHealthy is false:
            case CheckStatus.Error when _configuration.NotifyError is false:
            case CheckStatus.Warning when _configuration.NotifyWarning is false:
            case CheckStatus.Unknown when _configuration.NotifyUnknown is false:
                return;
        }

        string title = checkResult.Status switch
        {
            CheckStatus.Healthy => "Check returned healthy",
            CheckStatus.Warning => "Check returned warning",
            CheckStatus.Error => "Check returned error",
            CheckStatus.Unknown => "Check returned unknown",
            _ => "Unknown status returned"
        };

        DColor dcolor = checkResult.Status switch
        {
            CheckStatus.Healthy => Colors.Green,
            CheckStatus.Warning => Colors.Yellow,
            CheckStatus.Error => Colors.Red,
            CheckStatus.Unknown => Colors.Purple,
            _ => Colors.Orange
        };

        await _notificationService.SendEmbedAsync(title, checkResult.Message, null, dcolor);
    }

    public async Task<IReadOnlyList<ICheckResult>> RunChecksAsync(CancellationToken cancellationToken = default)
    {
        List<ICheckResult> results = new List<ICheckResult>();
        foreach (var check in _checks)
        {
            if (_nextRunTimes.ContainsKey(check) && _nextRunTimes[check] > DateTime.Now)
            {
                _logger.LogTrace("Skipping check: {Check}, will run in {NextTime}", check.Name,  (_nextRunTimes[check] - DateTime.Now).Humanize());
                continue;
            }

            _logger.LogInformation("Running check: {Check}", check.Name);
            try
            {
                var result = await check.ExecuteAsync(cancellationToken);
                _logger.LogInformation("Finished running: {Check}", check.Name);
                results.Add(result);
                _nextRunTimes[check] = DateTime.Now + check.CheckInterval;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run check: {Check}", check.Name);
                _nextRunTimes[check] = DateTime.Now + check.CheckInterval + TimeSpan.FromMinutes(30);
                results.Add(new CheckResult(check.Name, CheckStatus.Error, "Failed to run check due to " + e.Message, null, DateTimeOffset.Now));
            }
        }
        return results;
    }
}