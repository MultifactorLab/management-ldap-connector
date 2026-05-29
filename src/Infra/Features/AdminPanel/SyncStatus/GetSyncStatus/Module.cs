using App.Features.AdminPanel.SyncStatus.GetSyncStatus;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.AdminPanel.SyncStatus.GetSyncStatus;

public static class Module
{
    public static void AddGetSyncStatusFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IGetSyncStatus, GetSyncStatusAdapter>();
    }
}