using Microsoft.Extensions.DependencyInjection;

namespace Infra.Integrations.MgmHttpClientFactory;

public static class Module
{
    public static void AddMgmHttpClientFactory(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IMgmHttpClientFactory, MgmHttpClientFactory>();
    }
}