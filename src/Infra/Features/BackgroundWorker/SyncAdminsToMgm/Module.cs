using App.Features.BackgroundWorker.SyncAdminsToMgm;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.BackgroundWorker.SyncAdminsToMgm;

public static class Module
{
    public static void AddSyncAdminsToMgmFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ISyncAdminsToMgm, MgmSyncAdminsAdapter>();
    }
}