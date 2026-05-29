using App.Features.AdminPanel.SyncSettings.GetSyncSettings;
using Infra.SharedAdapters.GetSyncSettings;

namespace Api.Features.AdminPanel.SyncSettings.GetSyncSettings;

internal static class Module
{
    public static void AddGetSyncSettingsFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddGetSyncSettingsFeatureApp();
        builder.Services.AddGetSyncSettingsFeatureInfra();
    }
}