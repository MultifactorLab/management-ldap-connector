using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.LdapSettings.SaveLdapSettings;

public static class Module
{
    public static void AddSaveLdapSettingsFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<ISaveLdapSettingsUseCase, SaveLdapSettingsUseCase>();
    }
}