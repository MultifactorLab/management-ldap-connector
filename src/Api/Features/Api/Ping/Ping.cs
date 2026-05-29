using Api.Framework.EndpointDiscovery;

namespace Api.Features.Api.Ping;

internal sealed class Ping : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpointsBuilder)
    {
        endpointsBuilder
            .MapGet("/api/ping", Handle)
            .AllowAnonymous();
    }

    public static PingResponseDto Handle()
    {
        return new PingResponseDto(Success: true, ServerTime: DateTimeOffset.Now);
    }
}

internal sealed record PingResponseDto(bool Success, DateTimeOffset ServerTime);