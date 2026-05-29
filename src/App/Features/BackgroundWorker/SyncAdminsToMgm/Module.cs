using Microsoft.Extensions.DependencyInjection;

namespace App.Features.BackgroundWorker.SyncAdminsToMgm;

public static class Module
{
    public static void AddSyncUsersFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<ISyncUsersUseCase, SyncAdminsToMgmUseCase>();
    }
}