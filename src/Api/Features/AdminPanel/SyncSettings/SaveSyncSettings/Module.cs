using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using Infra.Features.AdminPanel.SyncSettings.SaveSyncSettings;

namespace Api.Features.AdminPanel.SyncSettings.SaveSyncSettings;

internal static class Module
{
    public static void AddSaveSyncSettingsFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddSaveSyncSettingsFeatureApp();
        builder.Services.AddSaveSyncSettingsFeatureInfra();
    }
}