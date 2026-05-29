using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.SyncSettings.GetSyncSettings;

public static class Module
{
    public static void AddGetSyncSettingsFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<IGetSyncSettingsUseCase, GetSyncSettingsUseCase>();
    }
}