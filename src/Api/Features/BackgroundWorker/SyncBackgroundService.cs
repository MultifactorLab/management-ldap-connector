using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using App.Features.BackgroundWorker.SyncAdminsToMgm;
using App.SharedPorts;

namespace Api.Features.BackgroundWorker;

internal sealed class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISyncSettingsChangeNotifier _changeNotifier;
    private readonly ILogger<SyncBackgroundService> _logger;

    private const int DefaultIntervalMinutes = 60;

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ISyncSettingsChangeNotifier changeNotifier,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _changeNotifier = changeNotifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalMinutes = GetIntervalMinutes();
            _logger.LogInformation("Sync timer started with interval {Interval} minutes.", intervalMinutes);

            using var settingsChangedCts = CancellationTokenSource
                .CreateLinkedTokenSource(stoppingToken, _changeNotifier.ChangedToken);

            // Запускаем сразу при старте
            await RunSync(settingsChangedCts.Token);

            // Потом по таймеру
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

            try
            {
                while (await timer.WaitForNextTickAsync(settingsChangedCts.Token))
                {
                    await RunSync(settingsChangedCts.Token);
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Настройки изменились — перезапускаем таймер с новым интервалом
                _logger.LogInformation("Sync settings changed, restarting timer with new interval.");
            }
        }

        _logger.LogInformation("Sync background service stopped.");
    }

    private async Task RunSync(CancellationToken ct)
    {
        _logger.LogInformation("Starting scheduled sync.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var getSyncSettings = scope.ServiceProvider
                .GetRequiredService<IGetSyncSettings>();
            var settings = getSyncSettings.Execute();

            if (settings is null)
            {
                _logger.LogInformation("SyncSettings not configured, skipping.");
                return;
            }

            var syncUseCase = scope.ServiceProvider
                .GetRequiredService<ISyncUsersUseCase>();

            var result = await syncUseCase.Execute(ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Scheduled sync completed. Synced {Count} admins.", result.SyncedCount);
            }
            else
            {
                _logger.LogWarning("Scheduled sync failed: {Error:l}.", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during scheduled sync.");
        }
    }

    private int GetIntervalMinutes()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var getSyncSettings = scope.ServiceProvider
                .GetRequiredService<IGetSyncSettings>();

            var settings = getSyncSettings.Execute();
            return settings is { IntervalMinutes: > 0 }
                ? settings.IntervalMinutes
                : DefaultIntervalMinutes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get sync interval, using default {Default} minutes.",
                DefaultIntervalMinutes);
            return DefaultIntervalMinutes;
        }
    }
}