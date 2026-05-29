using App.SharedPorts.Encryption.DecryptValue;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.Encryption.DecryptValue;

public static class Module
{
    public static void AddDecryptValue(this IServiceCollection services)
    {
        services.AddTransient<IDecryptValue, DecryptValueAdapter>();
    }
}