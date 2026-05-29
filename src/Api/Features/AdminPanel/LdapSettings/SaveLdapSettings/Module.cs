using App.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using Infra.Features.AdminPanel.LdapSettings.SaveLdapSettings;

namespace Api.Features.AdminPanel.LdapSettings.SaveLdapSettings;

internal static class Module
{
    public static void AddSaveLdapSettingsFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddSaveLdapSettingsFeatureApp();
        builder.Services.AddSaveLdapSettingsFeatureInfra();
    }
}