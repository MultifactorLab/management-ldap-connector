using Infra.Integrations.LdapConnectionFactory;
using Infra.Integrations.MgmHttpClientFactory;
using Infra.SharedAdapters.Encryption;
using Infra.SharedAdapters.Encryption.DecryptValue;
using Infra.SharedAdapters.Encryption.EncryptValue;
using Infra.SharedAdapters.HashPassword;
using Infra.SharedAdapters.LdapExecutor;
using Infra.SharedAdapters.VerifyPassword;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.SharedAdapters;

public static class Module
{
    public static void AddSharedAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHasPassword();
        services.AddVerifyPassword();
        services.AddEncryption(configuration);
        services.AddLdapExecutor();
    }
}