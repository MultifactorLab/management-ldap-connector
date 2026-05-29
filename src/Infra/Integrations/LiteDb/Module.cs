using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Integrations.LiteDb;

public static class Module
{
    public static void AddLiteDbAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LiteDbConfig>(configuration.GetSection(LiteDbConfig.SectionName));
        services.AddSingleton<ILiteDbConnection, LiteDbConnection>();
    }
}