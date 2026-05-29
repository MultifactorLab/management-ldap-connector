using System.DirectoryServices.Protocols;
using System.Net;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using App.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate.Strategies;
using Infra.SharedAdapters.LdapExecutor;
using Infra.Tests.Features.BackgroundWorker.Helpers;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.Api;

public class SambaAuthenticationStrategyTests
{
    private readonly AutoMocker _mocker = new();
    private readonly SambaAuthenticationStrategy _strategy = new();
    private readonly LdapConnection _connection;
 
    public SambaAuthenticationStrategyTests()
    {
        _connection = (LdapConnection)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(LdapConnection));
    }
 
    [Fact]
    public void Authenticate_WhenUserFoundAndBindSucceeds_ReturnSuccess()
    {
        SetupSearchResponse(
            LdapTestHelpers.CreateEntry(
                "CN=User 05,CN=Users,DC=sambadomain,DC=local",
                ("displayName", "User 05")));
 
        var result = _strategy.Authenticate(
            _connection, BuildSettings(), "user05", "password",
            _mocker.Get<ILdapExecutor>());
 
        Assert.True(result.IsSuccess);
        Assert.Equal("User 05", result.DisplayName);
    }
 
    [Fact]
    public void Authenticate_WhenUserNotFound_ReturnUserNotFound()
    {
        SetupSearchResponse(); // пустой результат
 
        var result = _strategy.Authenticate(
            _connection, BuildSettings(), "unknown", "password",
            _mocker.Get<ILdapExecutor>());
 
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateLdapUserError.UserNotFound, result.Error);
    }
 
    [Fact]
    public void Authenticate_WhenUserBindFails_ReturnInvalidCredentials()
    {
        SetupSearchResponse(
            LdapTestHelpers.CreateEntry(
                "CN=User 05,CN=Users,DC=sambadomain,DC=local",
                ("displayName", "User 05")));
 
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.Bind(It.IsAny<LdapConnection>(), It.IsAny<NetworkCredential>()))
            .Throws(new LdapException(LdapErrorCodes.InvalidCredentials, "Invalid credentials"));
 
        var result = _strategy.Authenticate(
            _connection, BuildSettings(), "user05", "wrongpassword",
            _mocker.Get<ILdapExecutor>());
 
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticateLdapUserError.InvalidCredentials, result.Error);
    }
 
    [Fact]
    public void Authenticate_WhenSyncGroupDnIsNull_SearchWithoutMemberOfFilter()
    {
        SetupSearchResponse();
 
        _strategy.Authenticate(
            _connection, BuildSettings(syncGroupDn: null), "user05", "password",
            _mocker.Get<ILdapExecutor>());
 
        _mocker.GetMock<ILdapExecutor>()
            .Verify(x => x.SendRequest(
                _connection,
                It.Is<SearchRequest>(r =>
                    r.Filter.ToString()!.Contains("sAMAccountName=user05") &&
                    !r.Filter.ToString()!.Contains("memberOf"))),
                Times.Once);
    }
 
    [Fact]
    public void Authenticate_WhenSyncGroupDnIsProvided_SearchWithMemberOfFilter()
    {
        const string groupDn = "CN=mfa_users,CN=Users,DC=sambadomain,DC=local";
        SetupSearchResponse();
 
        _strategy.Authenticate(
            _connection, BuildSettings(syncGroupDn: groupDn), "user05", "password",
            _mocker.Get<ILdapExecutor>());
 
        _mocker.GetMock<ILdapExecutor>()
            .Verify(x => x.SendRequest(
                _connection,
                It.Is<SearchRequest>(r =>
                    r.Filter.ToString()!.Contains("sAMAccountName=user05") &&
                    r.Filter.ToString()!.Contains("memberOf"))),
                Times.Once);
    }
 
    [Fact]
    public void Authenticate_WhenDisplayNameMissing_UseUsernameAsDisplayName()
    {
        SetupSearchResponse(
            LdapTestHelpers.CreateEntry(
                "CN=user05,DC=company,DC=local"
                // нет displayName
            ));
 
        var result = _strategy.Authenticate(
            _connection, BuildSettings(), "user05", "password",
            _mocker.Get<ILdapExecutor>());
 
        Assert.True(result.IsSuccess);
        Assert.Equal("user05", result.DisplayName);
    }
 
    private void SetupSearchResponse(params SearchResultEntry[] entries)
    {
        var response = LdapTestHelpers.CreateSearchResponse(entries);
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.SendRequest(It.IsAny<LdapConnection>(), It.IsAny<SearchRequest>()))
            .Returns(response);
    }
 
    private static LdapSettingsModel BuildSettings(string? syncGroupDn = null) => new()
    {
        ServerUrl = "11.12.0.88",
        Port = 636,
        UseSsl = true,
        BaseDn = "DC=sambadomain,DC=local",
        ServiceAccountDn = "CN=Tech Service,CN=Users,DC=sambadomain,DC=local",
        BindPassword = "secret",
        UsernameAttribute = "sAMAccountName",
        EmailAttribute = "mail",
        DisplayNameAttribute = "displayName",
        SyncGroupDn = syncGroupDn
    };
}