using Microsoft.Extensions.DependencyInjection;

namespace Infra.Integrations.LdapConnectionFactory;

public static class Module
{
    public static void AddLdapConnectionFactory(this IServiceCollection services)
    {
        services.AddTransient<ILdapConnectionFactory, LdapConnectionFactory>();
    }
}