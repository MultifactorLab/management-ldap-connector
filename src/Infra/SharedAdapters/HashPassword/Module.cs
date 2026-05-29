using App.SharedPorts.HashPassword;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.HashPassword;

public static class Module
{
    public static void AddHasPassword(this IServiceCollection services)
    {
        services.AddTransient<IHashPassword, BCryptHashPasswordAdapter>();
    }
}