using Infra.Features.AdminPanel.SyncStatus.GetSyncStatus;

namespace Api.Features.AdminPanel.SyncStatus.GetSyncStatus;

internal static class Module
{
    public static void AddGetSyncStatusFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddGetSyncStatusFeatureInfra();
    }
}