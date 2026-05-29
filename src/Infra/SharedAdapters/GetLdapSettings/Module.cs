using App.SharedPorts;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.GetLdapSettings;

public static class Module
{
    public static void AddGetLdapSettingsFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IGetLdapSettings, GetLdapSettingsAdapter>();
    }
}