using App.Features.BackgroundWorker.GetAdminsFromMgm;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.BackgroundWorker.GetAdminsFromMgm;

public static class Module
{
    public static void AddGetAdminsFromMgmFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IGetAdminsFromMgm, MgmGetAdminsAdapter>();
    }
}