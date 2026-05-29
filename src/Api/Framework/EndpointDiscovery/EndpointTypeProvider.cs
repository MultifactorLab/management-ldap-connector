using System.Reflection;

namespace Api.Framework.EndpointDiscovery;

internal static class EndpointTypeProvider
{
    
    public static IEnumerable<Type> GetEndpointTypes()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .Where(HasInterface<IEndpoint>);
    }

    private static bool HasInterface<TInterface>(Type type)
    {
        return type.GetInterfaces().Any(i => i == typeof(TInterface));
    }
}