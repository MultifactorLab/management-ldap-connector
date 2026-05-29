using App.Features.BackgroundWorker.SyncAdminsToMgm;
using Infra.Features.BackgroundWorker.SyncAdminsToMgm;

namespace Api.Features.BackgroundWorker.SyncAdminsToMgm;

internal static class Module
{
    public static void AddSyncUsersFeatureFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddSyncUsersFeatureApp();
        builder.Services.AddSyncAdminsToMgmFeatureInfra();
    }
}