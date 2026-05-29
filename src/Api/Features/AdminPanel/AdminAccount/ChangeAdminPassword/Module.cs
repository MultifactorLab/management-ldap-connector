using App.Features.AdminPanel.AdminAccount.ChangeAdminPassword;

namespace Api.Features.AdminPanel.AdminAccount.ChangeAdminPassword;

internal static class Module
{
    public static void AddChangeAdminPasswordFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddChangeAdminPasswordFeatureApp();
    }
}