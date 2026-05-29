using Infra.Integrations.LdapConnectionFactory;
using Infra.Integrations.LiteDb;
using Infra.Integrations.MgmHttpClientFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Integrations;

public static class Module
{
    public static void AddIntegrationAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLiteDbAdapter(configuration);
        services.AddLdapConnectionFactory();
        services.AddMgmHttpClientFactory();
    }
}