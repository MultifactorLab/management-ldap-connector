using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using Infra.Features.AdminPanel.LdapSettings.TestLdapConnection;

namespace Api.Features.AdminPanel.LdapSettings.TestLdapConnection;

internal static class Module
{
    public static void AddTestLdapConnectionFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddTestLdapConnectionFeatureApp();
        builder.Services.AddTestLdapConnectionFeatureInfra();
    }
}