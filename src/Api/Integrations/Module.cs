using Infra.Integrations;

namespace Api.Integrations;

public static class Module
{
    public static void AddIntegrations(this WebApplicationBuilder builder)
    {
        builder.Services.AddIntegrationAdapters(builder.Configuration);
    }
}