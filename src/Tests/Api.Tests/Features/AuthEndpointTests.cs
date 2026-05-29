using Api.Features.Api.Authenticate;
using App.Features.Api.Authenticate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Moq.AutoMock;

namespace Api.Tests.Features;

public class AuthEndpointTests
{
    private readonly AutoMocker _mocker = new();
 
    [Fact]
    public void Handle_WhenSuccess_Returns200WithDisplayName()
    {
        _mocker.GetMock<IAuthenticateUseCase>()
            .Setup(x => x.Execute(It.IsAny<AuthenticateRequest>()))
            .Returns(AuthenticateResult.Success("Alex T"));
 
        var result = Auth.Handle(
            new AuthRequestDto { Username = "alex", Password = "password" },
            _mocker.Get<IAuthenticateUseCase>());
 
        var ok = Assert.IsType<Ok<AuthResponseDto>>(result);
        Assert.True(ok.Value!.Success);
        Assert.Equal("Alex T", ok.Value.DisplayName);
    }
 
    [Fact]
    public void Handle_WhenUserNotFound_Returns404()
    {
        _mocker.GetMock<IAuthenticateUseCase>()
            .Setup(x => x.Execute(It.IsAny<AuthenticateRequest>()))
            .Returns(AuthenticateResult.Failure(AuthenticateError.UserNotFound));
 
        var result = Auth.Handle(
            new AuthRequestDto { Username = "unknown", Password = "password" },
            _mocker.Get<IAuthenticateUseCase>());
 
        Assert.IsType<NotFound>(result);
    }
 
    [Fact]
    public void Handle_WhenInvalidCredentials_Returns401()
    {
        _mocker.GetMock<IAuthenticateUseCase>()
            .Setup(x => x.Execute(It.IsAny<AuthenticateRequest>()))
            .Returns(AuthenticateResult.Failure(AuthenticateError.InvalidCredentials));
 
        var result = Auth.Handle(
            new AuthRequestDto { Username = "alex", Password = "wrong" },
            _mocker.Get<IAuthenticateUseCase>());
 
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
 
    [Fact]
    public void Handle_WhenLdapUnavailable_Returns503()
    {
        _mocker.GetMock<IAuthenticateUseCase>()
            .Setup(x => x.Execute(It.IsAny<AuthenticateRequest>()))
            .Returns(AuthenticateResult.Failure(AuthenticateError.LdapUnavailable));
 
        var result = Auth.Handle(
            new AuthRequestDto { Username = "alex", Password = "password" },
            _mocker.Get<IAuthenticateUseCase>());
 
        var statusCode = Assert.IsType<StatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCode.StatusCode);
    }
 
    [Fact]
    public void Handle_WhenSettingsNotConfigured_Returns503()
    {
        _mocker.GetMock<IAuthenticateUseCase>()
            .Setup(x => x.Execute(It.IsAny<AuthenticateRequest>()))
            .Returns(AuthenticateResult.Failure(AuthenticateError.SettingsNotConfigured));
 
        var result = Auth.Handle(
            new AuthRequestDto { Username = "alex", Password = "password" },
            _mocker.Get<IAuthenticateUseCase>());
 
        var statusCode = Assert.IsType<StatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCode.StatusCode);
    }
}