using Infra.SharedAdapters.SaveAdminAccount;

namespace Api.Features.AdminPanel.AdminAccount.SaveAdminAccount;

internal static class Module
{
    public static void AddSaveAdminAccountFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddSaveAdminAccountFeatureInfra();
    }
}