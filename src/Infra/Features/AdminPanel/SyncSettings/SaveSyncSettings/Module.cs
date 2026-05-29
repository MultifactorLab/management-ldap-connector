using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.AdminPanel.SyncSettings.SaveSyncSettings;

public static class Module
{
    public static void AddSaveSyncSettingsFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ISaveSyncSettings, SaveSyncSettingsAdapter>();
    }
}