using App.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate;

namespace Api.Features.Api.Authenticate;

internal static class Module
{
    public static void AddAuthenticateFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthenticateFeatureApp();
        builder.Services.AddAuthenticateLdapUserFeatureInfra();
    }
}


    