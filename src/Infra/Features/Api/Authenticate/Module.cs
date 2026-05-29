using App.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.Api.Authenticate;

public static class Module
{
    public static void AddAuthenticateLdapUserFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IAuthenticateLdapUser, LdapAuthenticateAdapter>();
        services.AddSingleton<ILdapAuthenticationStrategy, SambaAuthenticationStrategy>();
    }
}