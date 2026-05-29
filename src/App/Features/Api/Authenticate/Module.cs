using Microsoft.Extensions.DependencyInjection;

namespace App.Features.Api.Authenticate;

public static class Module
{
    public static void AddAuthenticateFeatureApp(this IServiceCollection services)
    {
        services.AddTransient<IAuthenticateUseCase, AuthenticateUseCase>();
    }
}