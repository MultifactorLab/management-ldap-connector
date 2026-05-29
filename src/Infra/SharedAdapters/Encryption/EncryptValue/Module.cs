using App.SharedPorts.Encryption.EncryptValue;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.Encryption.EncryptValue;

public static class Module
{
    public static void AddEncryptValue(this IServiceCollection services)
    {
        services.AddTransient<IEncryptValue, EncryptValueAdapter>();
    }
}