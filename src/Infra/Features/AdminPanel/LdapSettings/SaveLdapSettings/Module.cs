using App.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.AdminPanel.LdapSettings.SaveLdapSettings;

public static class Module
{
    public static void AddSaveLdapSettingsFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<ISaveLdapSettings, SaveLdapSettingsAdapter>();
    }
}