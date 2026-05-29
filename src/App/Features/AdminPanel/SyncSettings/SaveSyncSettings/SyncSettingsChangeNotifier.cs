namespace App.Features.AdminPanel.SyncSettings.SaveSyncSettings;

/// <summary>
/// Уведомляет SyncBackgroundService об изменении настроек синхронизации.
/// После вызова NotifyChanged() фоновый сервис перезапускает таймер с новым интервалом.
/// </summary>
public interface ISyncSettingsChangeNotifier
{
    void NotifyChanged();
    CancellationToken ChangedToken { get; }
}
 
internal sealed class SyncSettingsChangeNotifier : ISyncSettingsChangeNotifier
{
    private readonly object _lock = new();
    private CancellationTokenSource _cts = new();

    public CancellationToken ChangedToken
    {
        get
        {
            lock (_lock)
            {
                return _cts.Token;
            }
        }
    }

    public void NotifyChanged()
    {
        CancellationTokenSource old;
        lock (_lock)
        {
            old = _cts;
            _cts = new CancellationTokenSource();
        }
        old.Cancel();
        old.Dispose();
    }
}