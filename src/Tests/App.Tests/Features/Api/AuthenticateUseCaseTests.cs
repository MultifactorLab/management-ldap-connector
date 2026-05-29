using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.Api.Authenticate;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.Api;

public class AuthenticateUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly IAuthenticateUseCase _useCase;

    public AuthenticateUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<AuthenticateUseCase>();
    }

    [Fact]
    public void Execute_WhenSuccess_ReturnDisplayName()
    {
        SetupSettings();
        SetupDecrypt("plainPassword");
        SetupAuthenticate(AuthenticateLdapUserResult.Success("Alex T"));

        var result = _useCase.Execute(new AuthenticateRequest("alex", "password"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Alex T", result.DisplayName);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Execute_WhenSettingsNotConfigured_ReturnSettingsNotConfigured()
    {
        var result = _useCase.Execute(new AuthenticateRequest("alex", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateError.SettingsNotConfigured, result.Error);
    }

    [Fact]
    public void Execute_WhenUserNotFound_ReturnUserNotFound()
    {
        SetupSettings();
        SetupDecrypt("plainPassword");
        SetupAuthenticate(AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.UserNotFound));

        var result = _useCase.Execute(new AuthenticateRequest("unknown", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateError.UserNotFound, result.Error);
    }

    [Fact]
    public void Execute_WhenInvalidCredentials_ReturnInvalidCredentials()
    {
        SetupSettings();
        SetupDecrypt("plainPassword");
        SetupAuthenticate(AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.InvalidCredentials));

        var result = _useCase.Execute(new AuthenticateRequest("alex", "wrongpassword"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateError.InvalidCredentials, result.Error);
    }

    [Fact]
    public void Execute_WhenConnectionFailed_ReturnLdapUnavailable()
    {
        SetupSettings();
        SetupDecrypt("plainPassword");
        SetupAuthenticate(AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.ConnectionFailed));

        var result = _useCase.Execute(new AuthenticateRequest("alex", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateError.LdapUnavailable, result.Error);
    }

    private void SetupSettings(string encryptedPassword = "$encrypted$") =>
        _mocker.GetMock<IGetLdapSettings>()
            .Setup(x => x.Execute())
            .Returns(new GetLdapSettingsModel()
            {
                ServerUrl = "10.27.0.80",
                Port = 636,
                UseSsl = true,
                BaseDn = "DC=company,DC=local",
                ServiceAccountDn = "CN=svc,DC=company,DC=local",
                BindPassword = encryptedPassword,
                UsernameAttribute = "sAMAccountName",
                EmailAttribute = "mail",
                DisplayNameAttribute = "displayName",
                SyncGroupDn = null
            });

    private void SetupDecrypt(string plainText) =>
        _mocker.GetMock<IDecryptValue>()
            .Setup(x => x.Execute(It.IsAny<DecryptValueRequest>()))
            .Returns(new DecryptValueResult(plainText));

    private void SetupAuthenticate(AuthenticateLdapUserResult result) =>
        _mocker.GetMock<IAuthenticateLdapUser>()
            .Setup(x => x.Execute(
                It.IsAny<LdapSettingsModel>(),
                It.IsAny<UserCredential>()))
            .Returns(result);
}