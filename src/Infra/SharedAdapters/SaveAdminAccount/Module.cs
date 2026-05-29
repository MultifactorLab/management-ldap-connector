using App.SharedPorts;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.SaveAdminAccount;

public static class Module
{
    public static void AddSaveAdminAccountFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ISaveAdminAccount, SaveAdminAccountAdapter>();
    }
}