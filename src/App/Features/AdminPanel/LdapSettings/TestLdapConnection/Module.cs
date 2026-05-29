using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.LdapSettings.TestLdapConnection;

public static class Module
{
    public static void AddTestLdapConnectionFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<ITestLdapConnectionUseCase, TestLdapConnectionUseCase>();
    }
}