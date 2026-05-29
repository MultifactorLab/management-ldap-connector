using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.LdapSettings.GetLdapSettings;

public static class Module
{
    public static void AddGetLdapSettingsFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<IGetLdapSettingsUseCase, GetLdapSettingsUseCase>();
    }
}