using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.AdminAccount.ChangeAdminPassword;

public static class Module
{
    public static void AddChangeAdminPasswordFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<IChangeAdminPasswordUseCase, ChangeAdminPasswordUseCase>();
    }
}