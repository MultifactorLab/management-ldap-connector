using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.AdminPanel.LdapSettings.TestLdapConnection;

public static class Module
{
    public static void AddTestLdapConnectionFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ITestLdapConnection, LdapTestConnectionAdapter>();
    }
}