using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.SyncSettings.SaveSyncSettings;

public static class Module
{
    public static void AddSaveSyncSettingsFeatureApp(this IServiceCollection services)
    {
        // нужен именно Singleton, для того чтобы был один инстанс для обновления таймера в SyncBackgroundService 
        services.AddSingleton<ISyncSettingsChangeNotifier, SyncSettingsChangeNotifier>();
        services.AddTransient<ISaveSyncSettingsUseCase, SaveSyncSettingsUseCase>();
    }
}