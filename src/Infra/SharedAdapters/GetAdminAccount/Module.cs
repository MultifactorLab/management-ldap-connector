using App.SharedPorts;
using Infra.Features.AdminPanel.AdminAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.GetAdminAccount;

public static class Module
{
    public static void AddGetAdminAccountFeatureInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminConfig>(configuration.GetSection(AdminConfig.SectionName));
        
        services.AddTransient<IGetAdminAccount, GetAdminAccountAdapter>();
    }
}