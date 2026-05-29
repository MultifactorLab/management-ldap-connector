using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.LdapExecutor;

public static class Module
{
    public static void AddLdapExecutor(this IServiceCollection services)
    {
        services.AddTransient<ILdapExecutor, LdapExecutor>();
    }
}