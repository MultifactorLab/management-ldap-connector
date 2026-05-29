using Api.Features.AdminPanel;
using Api.Features.Api;
using Api.Features.BackgroundWorker;

namespace Api.Features;

internal static class Module
{
    public static void AddFeatures(this WebApplicationBuilder builder)
    {
        builder.AddAdminPanelFeatures();
        builder.AddApiFeatures();
        builder.AddBackgroundWorkerFeatures();
    }
}