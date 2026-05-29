using App.SharedPorts;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.GetSyncSettings;

public static class Module
{
    public static void AddGetSyncSettingsFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IGetSyncSettings, GetSyncSettingsAdapter>();
    }
}