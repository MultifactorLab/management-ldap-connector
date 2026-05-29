using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.CompilerServices;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Infra.Features.AdminPanel.LdapSettings.TestLdapConnection;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.AdminPanel.LdapSettings;

public class LdapTestConnectionAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly LdapTestConnectionAdapter _adapter;
    private readonly LdapConnection _connection;
 
    public LdapTestConnectionAdapterTests()
    {
        _connection = CreateFakeConnection();
        _mocker.GetMock<ILdapConnectionFactory>()
            .Setup(x => x.Create(It.IsAny<LdapConnectionFactoryModel>()))
            .Returns(_connection);
 
        _adapter = _mocker.CreateInstance<LdapTestConnectionAdapter>();
    }
 
    [Fact]
    public void Execute_WhenBindSucceeds_ReturnSuccess()
    {
        var result = _adapter.Execute(BuildSettings());
 
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }
 
    [Fact]
    public void Execute_WhenBindSucceeds_CallBindWithCorrectCredentials()
    {
        var settings = BuildSettings(serviceAccountDn: "CN=svc,DC=company,DC=local", bindPassword: "secret");
 
        _adapter.Execute(settings);
 
        _mocker.GetMock<ILdapExecutor>()
            .Verify(x => x.Bind(
                _connection,
                It.Is<NetworkCredential>(c =>
                    c.UserName == "CN=svc,DC=company,DC=local" &&
                    c.Password == "secret")),
                Times.Once);
    }
 
    [Fact]
    public void Execute_WhenLdapExceptionCode49_ReturnInvalidCredentials()
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new LdapException(LdapErrorCodes.InvalidCredentials, "Invalid credentials"));
 
        var result = _adapter.Execute(BuildSettings());
 
        Assert.False(result.IsSuccess);
        Assert.Equal(TestLdapConnectionError.InvalidCredentials, result.Error);
    }
 
    [Fact]
    public void Execute_WhenLdapExceptionTimeout_ReturnConnectionFailed()
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new LdapException(LdapErrorCodes.TimedOut, "Timed out"));
 
        var result = _adapter.Execute(BuildSettings());
 
        Assert.False(result.IsSuccess);
        Assert.Equal(TestLdapConnectionError.ConnectionFailed, result.Error);
        Assert.Equal("Timed out", result.ErrorDetails);
    }
 
    [Fact]
    public void Execute_WhenUnexpectedException_ReturnConnectionFailed()
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new InvalidOperationException("Network error"));
 
        var result = _adapter.Execute(BuildSettings());
 
        Assert.False(result.IsSuccess);
        Assert.Equal(TestLdapConnectionError.ConnectionFailed, result.Error);
    }
 
    private static LdapConnection CreateFakeConnection() =>
        (LdapConnection)RuntimeHelpers
            .GetUninitializedObject(typeof(LdapConnection));
 
    private static TestLdapConnectionModel BuildSettings(
        string serviceAccountDn = "CN=svc,DC=company,DC=local",
        string bindPassword = "secret") => new()
    {
        ServerUrl = "11.12.0.88",
        Port = 636,
        UseSsl = true,
        ServiceAccountDn = serviceAccountDn,
        BindPassword = bindPassword,
    };
}