using Api.Features.BackgroundWorker.GetAdminsFromMgm;
using Api.Features.BackgroundWorker.SyncAdminsToMgm;
using Infra.Features.BackgroundWorker.GetUsersFromActiveDirectory;

namespace Api.Features.BackgroundWorker;

internal static class Module
{
    public static void AddBackgroundWorkerFeatures(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<SyncBackgroundService>();
        builder.Services.AddGetUsersFromActiveDirectoryFeatureInfra();
        builder.AddGetAdminsFromMgmFeature();
        builder.AddSyncUsersFeatureFeature();
    }
}