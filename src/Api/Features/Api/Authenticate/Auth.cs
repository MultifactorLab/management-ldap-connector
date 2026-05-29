using Api.Framework.EndpointDiscovery;
using App.Features.Api.Authenticate;

namespace Api.Features.Api.Authenticate;

public class Auth : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpointsBuilder)
    {
        endpointsBuilder
            .MapPost("/api/auth", Handle)
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes("Basic")
                .RequireAuthenticatedUser());
    }

    public static IResult Handle(AuthRequestDto request,
        IAuthenticateUseCase useCase)
    {
        var result = useCase.Execute(new AuthenticateRequest(
            request.Username,
            request.Password));

        if (result.IsSuccess)
        {
            return Results.Ok(AuthResponseDto.FromResult(result));
        }

        return result.Error switch
        {
            AuthenticateError.UserNotFound => Results.NotFound(),
            AuthenticateError.InvalidCredentials => Results.Unauthorized(),
            AuthenticateError.LdapUnavailable => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
            AuthenticateError.SettingsNotConfigured => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

public sealed class AuthRequestDto
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed class AuthResponseDto
{
    public bool Success { get; init; }
    public string? DisplayName { get; init; }

    public static AuthResponseDto FromResult(AuthenticateResult result) => new()
    {
        Success = result.IsSuccess,
        DisplayName = result.DisplayName
    };
}