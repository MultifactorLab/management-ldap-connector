using App.Features.AdminPanel.Login;

namespace Api.Features.AdminPanel.Login;

internal static class Module
{
    public static void AddLoginFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddLoginFeatureApp();
    }
}