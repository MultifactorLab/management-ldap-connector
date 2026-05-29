using App.Features.BackgroundWorker.GetUsersFromActiveDirectory;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Features.BackgroundWorker.GetUsersFromActiveDirectory;

public static class Module
{
    public static void AddGetUsersFromActiveDirectoryFeatureInfra(this IServiceCollection services)
    {
        services.AddTransient<IGetUsersFromActiveDirectory, GetUsersFromActiveDirectoryAdapter>();
    }
}