using Api.Features.Api.Authenticate;

namespace Api.Features.Api;

internal static class Module
{
    public static void AddApiFeatures(this WebApplicationBuilder builder)
    {
        builder.AddAuthenticateFeature();
    }
}