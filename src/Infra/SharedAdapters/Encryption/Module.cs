using Infra.SharedAdapters.Encryption.DecryptValue;
using Infra.SharedAdapters.Encryption.EncryptValue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters.Encryption;

public static class Module
{
    public static void AddEncryption(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EncryptionConfig>(configuration.GetSection(EncryptionConfig.SectionName));
        
        services.AddDecryptValue();
        services.AddEncryptValue();
    }
}