using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.CompilerServices;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate.Strategies;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.Api;

public class LdapAuthenticateAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly LdapAuthenticateAdapter _sut;
    private readonly LdapConnection _connection;

    public LdapAuthenticateAdapterTests()
    {
        _connection = CreateFakeConnection();
        _mocker.GetMock<ILdapConnectionFactory>()
            .Setup(x => x.Create(It.IsAny<LdapConnectionFactoryModel>()))
            .Returns(_connection);

        _sut = _mocker.CreateInstance<LdapAuthenticateAdapter>();
    }

    [Fact]
    public void Execute_WhenStrategyReturnSuccess_ReturnSuccess()
    {
        _mocker.GetMock<ILdapAuthenticationStrategy>()
            .Setup(x => x.Authenticate(
                It.IsAny<LdapConnection>(),
                It.IsAny<LdapSettingsModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ILdapExecutor>()))
            .Returns(AuthenticateLdapUserResult.Success("Alex T"));

        var result = _sut.Execute(BuildSettings(), new UserCredential("alex", "password"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Alex T", result.DisplayName);
    }

    [Fact]
    public void Execute_WhenStrategyReturnFailure_ReturnFailure()
    {
        _mocker.GetMock<ILdapAuthenticationStrategy>()
            .Setup(x => x.Authenticate(
                It.IsAny<LdapConnection>(),
                It.IsAny<LdapSettingsModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ILdapExecutor>()))
            .Returns(AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.UserNotFound));

        var result = _sut.Execute(BuildSettings(), new UserCredential("unknown", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateLdapUserError.UserNotFound, result.Error);
    }

    [Fact]
    public void Execute_WhenBindFails_ReturnInvalidCredentials()
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new LdapException(LdapErrorCodes.InvalidCredentials, "Invalid credentials"));

        var result = _sut.Execute(BuildSettings(), new UserCredential("alex", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateLdapUserError.InvalidCredentials, result.Error);
    }

    [Fact]
    public void Execute_WhenConnectionFails_ReturnConnectionFailed()
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new LdapException(LdapErrorCodes.TimedOut, "Timed out"));

        var result = _sut.Execute(BuildSettings(), new UserCredential("alex", "password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateLdapUserError.ConnectionFailed, result.Error);
    }

    [Fact]
    public void Execute_WhenCalled_PassCorrectCredentialsToExecutor()
    {
        var settings = BuildSettings(serviceAccountDn: "CN=svc,DC=company,DC=local", bindPassword: "secret");
        _mocker.GetMock<ILdapAuthenticationStrategy>()
            .Setup(x => x.Authenticate(
                It.IsAny<LdapConnection>(),
                It.IsAny<LdapSettingsModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ILdapExecutor>()))
            .Returns(AuthenticateLdapUserResult.Success("Alex T"));

        _sut.Execute(settings, new UserCredential("alex", "password"));

        _mocker.GetMock<ILdapExecutor>()
            .Verify(x => x.Bind(
                    _connection,
                    It.Is<NetworkCredential>(c =>
                        c.UserName == "CN=svc,DC=company,DC=local" &&
                        c.Password == "secret")),
                Times.Once);
    }

    [Fact]
    public void Execute_WhenCalled_DelegateAuthenticationToStrategy()
    {
        _mocker.GetMock<ILdapAuthenticationStrategy>()
            .Setup(x => x.Authenticate(
                It.IsAny<LdapConnection>(),
                It.IsAny<LdapSettingsModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ILdapExecutor>()))
            .Returns(AuthenticateLdapUserResult.Success("Alex T"));

        _sut.Execute(BuildSettings(), new UserCredential("alex", "password"));

        _mocker.GetMock<ILdapAuthenticationStrategy>()
            .Verify(x => x.Authenticate(
                    _connection,
                    It.IsAny<LdapSettingsModel>(),
                    "alex",
                    "password",
                    It.IsAny<ILdapExecutor>()),
                Times.Once);
    }

    private static LdapConnection CreateFakeConnection() =>
        (LdapConnection)RuntimeHelpers.GetUninitializedObject(typeof(LdapConnection));

    private static LdapSettingsModel BuildSettings(
        string serviceAccountDn = "CN=svc,DC=company,DC=local",
        string bindPassword = "secret") => new()
    {
        ServerUrl = "11.12.0.88",
        Port = 636,
        UseSsl = true,
        BaseDn = "DC=company,DC=local",
        ServiceAccountDn = serviceAccountDn,
        BindPassword = bindPassword,
        UsernameAttribute = "sAMAccountName",
        EmailAttribute = "mail",
        DisplayNameAttribute = "displayName"
    };
}