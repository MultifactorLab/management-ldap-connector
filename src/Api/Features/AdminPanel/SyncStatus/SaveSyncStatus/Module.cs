using Infra.Features.AdminPanel.SyncStatus.SaveSyncStatus;

namespace Api.Features.AdminPanel.SyncStatus.SaveSyncStatus;

internal static class Module
{
    public static void AddSaveSyncStatusFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddSaveSyncStatusFeatureInfra();
    }
}