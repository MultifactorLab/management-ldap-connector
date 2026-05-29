using App.Features.AdminPanel.LdapSettings.GetLdapSettings;
using Infra.SharedAdapters.GetLdapSettings;

namespace Api.Features.AdminPanel.LdapSettings.GetLdapSettings;

internal static class Module
{
    public static void AddGetLdapSettingsFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddGetLdapSettingsFeatureApp();
        builder.Services.AddGetLdapSettingsFeatureInfra();
    }
}