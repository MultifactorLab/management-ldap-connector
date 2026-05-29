using Infra.Features.BackgroundWorker.GetAdminsFromMgm;

namespace Api.Features.BackgroundWorker.GetAdminsFromMgm;

internal static class Module
{
    public static void AddGetAdminsFromMgmFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddGetAdminsFromMgmFeatureInfra();
    }
}