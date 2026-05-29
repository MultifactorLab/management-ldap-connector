using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.AdminPanel.LdapSettings;

public class TestLdapConnectionUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ITestLdapConnectionUseCase _useCase;

    public TestLdapConnectionUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<TestLdapConnectionUseCase>();
    }

    [Fact]
    public void Execute_WhenConnectionSucceeds_ReturnSuccess()
    {
        _mocker.GetMock<ITestLdapConnection>()
            .Setup(x => x.Execute(It.IsAny<TestLdapConnectionModel>()))
            .Returns(TestLdapConnectionResult.Success());

        var result = _useCase.Execute(BuildSettings());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Execute_WhenConnectionFails_ReturnConnectionFailed()
    {
        _mocker.GetMock<ITestLdapConnection>()
            .Setup(x => x.Execute(It.IsAny<TestLdapConnectionModel>()))
            .Returns(TestLdapConnectionResult.Failure(
                TestLdapConnectionError.ConnectionFailed, "Connection refused"));

        var result = _useCase.Execute(BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal(TestLdapConnectionError.ConnectionFailed, result.Error);
        Assert.Equal("Connection refused", result.ErrorDetails);
    }

    [Fact]
    public void Execute_WhenInvalidCredentials_ReturnInvalidCredentials()
    {
        _mocker.GetMock<ITestLdapConnection>()
            .Setup(x => x.Execute(It.IsAny<TestLdapConnectionModel>()))
            .Returns(TestLdapConnectionResult.Failure(
                TestLdapConnectionError.InvalidCredentials));

        var result = _useCase.Execute(BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal(TestLdapConnectionError.InvalidCredentials, result.Error);
    }

    private static TestLdapConnectionModel BuildSettings(string serverUrl = "ldap://company.local") =>
        new()
        {
            ServerUrl = serverUrl,
            Port = 389,
            UseSsl = false,
            ServiceAccountDn = "CN=svc,DC=company,DC=local",
            BindPassword = "plainPassword",
        };
}