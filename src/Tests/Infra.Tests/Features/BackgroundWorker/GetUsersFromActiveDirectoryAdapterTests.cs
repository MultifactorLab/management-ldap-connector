using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Infra.Features.BackgroundWorker.GetUsersFromActiveDirectory;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;
using Infra.Tests.Features.BackgroundWorker.Helpers;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.BackgroundWorker;

public class GetUsersFromActiveDirectoryAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly GetUsersFromActiveDirectoryAdapter _fromActiveDirectoryAdapter;
    private readonly LdapConnection _connection;
 
    public GetUsersFromActiveDirectoryAdapterTests()
    {
        _connection = CreateFakeConnection();
        _mocker.GetMock<ILdapConnectionFactory>()
            .Setup(x => x.Create(It.IsAny<LdapConnectionFactoryModel>()))
            .Returns(_connection);
 
        _fromActiveDirectoryAdapter = _mocker.CreateInstance<GetUsersFromActiveDirectoryAdapter>();
    }
 
    [Fact]
    public void Execute_WhenSyncGroupDnIsNull_SearchAllUsersInBaseDn()
    {
        SetupSearchResponse(LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry("CN=User1,DC=company,DC=local",
                ("sAMAccountName", "user1"),
                ("mail", "user1@company.local"),
                ("displayName", "User One")),
            LdapTestHelpers.CreateEntry("CN=User2,DC=company,DC=local",
                ("sAMAccountName", "user2"),
                ("mail", "user2@company.local"),
                ("displayName", "User Two"))));
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(syncGroupDn: null));
 
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u is { Username: "user1", Email: "user1@company.local" });
        Assert.Contains(result, u => u is { Username: "user2", Email: "user2@company.local" });
    }
 
    [Fact]
    public void Execute_WhenSyncGroupDnIsNull_UseObjectClassPersonFilter()
    {
        SetupSearchResponse(LdapTestHelpers.CreateSearchResponse());
 
        _fromActiveDirectoryAdapter.Execute(BuildSettings(syncGroupDn: null));
 
        _mocker.GetMock<ILdapExecutor>()
            .Verify(x => x.SendRequest(
                _connection,
                It.Is<SearchRequest>(r => r.Filter.ToString()!.Contains("objectClass=person"))),
                Times.Once);
    }
 
    [Fact]
    public void Execute_WhenSyncGroupDnIsProvided_ReturnOnlyGroupMembers()
    {
        const string groupDn = "CN=MGM Admins,OU=Groups,DC=company,DC=local";
        const string memberDn = "CN=User1,CN=Users,DC=company,DC=local";
 
        // Первый запрос — группа с членами
        var groupResponse = LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry(groupDn, ("member", memberDn)));
 
        // Второй запрос — данные пользователя
        var userResponse = LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry(memberDn,
                ("sAMAccountName", "user1"),
                ("mail", "user1@company.local"),
                ("displayName", "User One")));
 
        var callCount = 0;
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.SendRequest(It.IsAny<LdapConnection>(), It.IsAny<SearchRequest>()))
            .Returns(() => callCount++ == 0 ? groupResponse : userResponse);
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(syncGroupDn: groupDn));
 
        Assert.Single(result);
        Assert.Equal("user1", result[0].Username);
        Assert.Equal("user1@company.local", result[0].Email);
        Assert.Equal("User One", result[0].DisplayName);
    }
 
    [Fact]
    public void Execute_WhenGroupNotFound_ReturnEmptyList()
    {
        SetupSearchResponse(LdapTestHelpers.CreateSearchResponse()); // пустой ответ
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(
            syncGroupDn: "CN=NonExistent,DC=company,DC=local"));
 
        Assert.Empty(result);
    }
 
    [Fact]
    public void Execute_WhenGroupHasNoMembers_ReturnEmptyList()
    {
        var groupResponse = LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry(
                "CN=EmptyGroup,DC=company,DC=local"
                // нет атрибута member
            ));
 
        SetupSearchResponse(groupResponse);
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(
            syncGroupDn: "CN=EmptyGroup,DC=company,DC=local"));
 
        Assert.Empty(result);
    }
 
    [Fact]
    public void Execute_WhenUserMissingEmail_SkipUser()
    {
        SetupSearchResponse(LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry("CN=User1,DC=company,DC=local",
                ("sAMAccountName", "user1")
                // нет mail
            )));
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(syncGroupDn: null));
 
        Assert.Empty(result);
    }
 
    [Fact]
    public void Execute_WhenDisplayNameMissing_UseUsernameAsDisplayName()
    {
        SetupSearchResponse(LdapTestHelpers.CreateSearchResponse(
            LdapTestHelpers.CreateEntry("CN=User1,DC=company,DC=local",
                ("sAMAccountName", "user1"),
                ("mail", "user1@company.local")
                // нет displayName
            )));
 
        var result = _fromActiveDirectoryAdapter.Execute(BuildSettings(syncGroupDn: null));
 
        Assert.Single(result);
        Assert.Equal("user1", result[0].DisplayName);
    }
 
    private void SetupSearchResponse(SearchResponse response)
    {
        _mocker.GetMock<ILdapExecutor>()
            .Setup(x => x.SendRequest(It.IsAny<LdapConnection>(), It.IsAny<SearchRequest>()))
            .Returns(response);
    }
 
    private static LdapConnection CreateFakeConnection() =>
        (LdapConnection)RuntimeHelpers
            .GetUninitializedObject(typeof(LdapConnection));
 
    private static LdapSettingsModel BuildSettings(string? syncGroupDn = null) => new()
    {
        ServerUrl = "11.22.0.88",
        Port = 636,
        UseSsl = true,
        BaseDn = "DC=company,DC=local",
        ServiceAccountDn = "CN=svc,DC=company,DC=local",
        BindPassword = "secret",
        UsernameAttribute = "sAMAccountName",
        EmailAttribute = "mail",
        DisplayNameAttribute = "displayName",
        SyncGroupDn = syncGroupDn
    };
}