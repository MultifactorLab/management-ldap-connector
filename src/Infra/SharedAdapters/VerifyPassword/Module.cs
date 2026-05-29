using App.SharedPorts.VerifyPassword;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.VerifyPassword;

public static class Module
{
    public static void AddVerifyPassword(this IServiceCollection services)
    {
        services.AddTransient<IVerifyPassword, BCryptVerifyPasswordAdapter>();
    }
}