namespace Api.Framework.EndpointDiscovery;

internal static class EndpointMapper
{
    /// <summary>
    /// Сканирует сборку на предмет классов, которые реализуют <see cref="IEndpoint"/>, и вызывает <see cref="IEndpoint.Map"/> для каждого из них.
    /// </summary>
    /// <param name="builder">Экземпляр <see cref="IEndpointRouteBuilder"/>. Например, <see cref="WebApplicationBuilder"/>.</param>
    public static void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var types = EndpointTypeProvider.GetEndpointTypes();
        var endpoints = types.Select(EndpointActivator.Activate);
        foreach (var endpoint in endpoints)
        {
            endpoint.Map(builder);
        }
    }
}