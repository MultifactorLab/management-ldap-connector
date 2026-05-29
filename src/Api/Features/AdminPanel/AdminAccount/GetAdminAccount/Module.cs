using Infra.SharedAdapters.GetAdminAccount;

namespace Api.Features.AdminPanel.AdminAccount.GetAdminAccount;

internal static class Module
{
    public static void AddGetAdminAccountFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddGetAdminAccountFeatureInfra(builder.Configuration);
    }
}