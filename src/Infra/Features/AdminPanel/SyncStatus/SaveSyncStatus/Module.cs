using App.Features.AdminPanel.SyncStatus.SaveSyncStatus;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.AdminPanel.SyncStatus.SaveSyncStatus;

public static class Module
{
    public static void AddSaveSyncStatusFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ISaveSyncStatus, SaveSyncStatusAdapter>();
    }
}