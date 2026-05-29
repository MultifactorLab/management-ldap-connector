namespace Api.Framework.EndpointDiscovery;

/// <summary>
/// Контракт для всех minimal-api ендпоинтов.
/// </summary>
internal interface IEndpoint
{
    void Map(IEndpointRouteBuilder endpointsBuilder);
}