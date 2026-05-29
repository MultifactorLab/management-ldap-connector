using Microsoft.Extensions.DependencyInjection;

namespace App.Features.AdminPanel.Login;

public static class Module
{
    public static void AddLoginFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<ILoginUseCase, LoginUseCase>();
    }
}