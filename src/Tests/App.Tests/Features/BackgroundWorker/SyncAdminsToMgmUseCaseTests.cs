using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.SyncSettings.GetSyncSettings.Models;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;
using App.Features.BackgroundWorker.GetAdminsFromMgm;
using App.Features.BackgroundWorker.GetUsersFromActiveDirectory;
using App.Features.BackgroundWorker.SyncAdminsToMgm;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.BackgroundWorker;

public class SyncAdminsToMgmUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ISyncUsersUseCase _useCase;

    public SyncAdminsToMgmUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<SyncAdminsToMgmUseCase>();
    }

    [Fact]
    public async Task Execute_WhenSyncSettingsNotConfigured_ReturnFailure()
    {
        var result = await _useCase.Execute();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WhenSyncSettingsNotConfigured_SaveFailureStatus()
    {
        await _useCase.Execute();

        _mocker.GetMock<ISaveSyncStatus>()
            .Verify(x => x.Execute(
                    It.Is<SaveSyncStatusModel>(m => !m.Success && m.ErrorMessage != null)),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenLdapSettingsNotConfigured_ReturnFailure()
    {
        SetupSyncSettings();

        var result = await _useCase.Execute();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WhenLdapUsersEmpty_ReturnNoChanges()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers();

        var result = await _useCase.Execute();

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.SyncedCount);
    }

    [Fact]
    public async Task Execute_WhenLdapUsersEmpty_DoesNotCallSyncAdmins()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers();

        await _useCase.Execute();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                    It.IsAny<IReadOnlyList<AdminDefinition>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task Execute_WhenLdapUsersEmpty_SaveStatusWithZeroCount()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers();

        await _useCase.Execute();

        _mocker.GetMock<ISaveSyncStatus>()
            .Verify(x => x.Execute(
                    It.Is<SaveSyncStatusModel>(m => m.Success && m.SyncedCount == 0)),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenAdminExistsButNameChanged_SyncUpdatedAdmin()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "New Name", "user1"));
        SetupMgmAdmins(new MgmAdmin("user1@company.local", "Old Name", "user1"));
 
        await _useCase.Execute();
 
        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                It.Is<IReadOnlyList<AdminDefinition>>(list =>
                    list.Count == 1 &&
                    list[0].Email == "user1@company.local" &&
                    list[0].Name == "New Name"),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }
 
    [Fact]
    public async Task Execute_WhenAdminExistsButLdapUsernameChanged_SyncUpdatedAdmin()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1_new"));
        SetupMgmAdmins(new MgmAdmin("user1@company.local", "User One", "user1_old"));
 
        await _useCase.Execute();
 
        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                It.Is<IReadOnlyList<AdminDefinition>>(list =>
                    list.Count == 1 &&
                    list[0].LdapUsername == "user1_new"),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }
 
    [Fact]
    public async Task Execute_WhenAdminExistsWithNoChanges_DoesNotSync()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));
        SetupMgmAdmins(new MgmAdmin("user1@company.local", "User One", "user1"));
 
        await _useCase.Execute();
 
        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                It.IsAny<IReadOnlyList<AdminDefinition>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task Execute_WhenNewAdminsFound_SyncThem()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));
        SetupMgmAdmins();

        await _useCase.Execute();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                    It.Is<IReadOnlyList<AdminDefinition>>(list =>
                        list.Count == 1 &&
                        list[0].Email == "user1@company.local"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNewAdminsFound_ReturnSuccessWithCount()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(
            new LdapUser("user1@company.local", "User One", "user1"),
            new LdapUser("user2@company.local", "User Two", "user2"));
        SetupMgmAdmins();

        var result = await _useCase.Execute();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.SyncedCount);
    }

    [Fact]
    public async Task Execute_WhenNewAdminsFound_MapAttributesCorrectly()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));
        SetupMgmAdmins();

        await _useCase.Execute();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                    It.Is<IReadOnlyList<AdminDefinition>>(list =>
                        list[0].Email == "user1@company.local" &&
                        list[0].Name == "User One" &&
                        list[0].LdapUsername == "user1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenLdapUserHasEmptyEmail_SkipUser()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(
            new LdapUser(string.Empty, "User One", "user1"), // пустой email
            new LdapUser("user2@company.local", "User Two", "user2"));
        SetupMgmAdmins();

        await _useCase.Execute();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                    It.Is<IReadOnlyList<AdminDefinition>>(list =>
                        list.Count == 1 &&
                        list[0].Email == "user2@company.local"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenLdapUserHasEmptyUsername_SkipUser()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", string.Empty));
        SetupMgmAdmins();

        await _useCase.Execute();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Verify(x => x.Execute(
                    It.IsAny<IReadOnlyList<AdminDefinition>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task Execute_WhenGetAdminsFromMgmFails_ReturnFailure()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));

        _mocker.GetMock<IGetAdminsFromMgm>()
            .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("MGM unavailable"));

        var result = await _useCase.Execute();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_WhenGetAdminsFromMgmFails_SaveFailureStatus()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));

        _mocker.GetMock<IGetAdminsFromMgm>()
            .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("MGM unavailable"));

        await _useCase.Execute();

        _mocker.GetMock<ISaveSyncStatus>()
            .Verify(x => x.Execute(
                    It.Is<SaveSyncStatusModel>(m => !m.Success && m.ErrorMessage != null)),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenSyncAdminsToMgmFails_SaveFailureStatus()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(new LdapUser("user1@company.local", "User One", "user1"));
        SetupMgmAdmins();

        _mocker.GetMock<ISyncAdminsToMgm>()
            .Setup(x => x.Execute(
                It.IsAny<IReadOnlyList<AdminDefinition>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("MGM unavailable"));

        await _useCase.Execute();

        _mocker.GetMock<ISaveSyncStatus>()
            .Verify(x => x.Execute(
                    It.Is<SaveSyncStatusModel>(m => !m.Success && m.ErrorMessage != null)),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WhenSucceeds_SaveStatusWithCorrectCount()
    {
        SetupSyncSettings();
        SetupLdapSettings();
        SetupDecrypt();
        SetupLdapUsers(
            new LdapUser("user1@company.local", "User One", "user1"),
            new LdapUser("user2@company.local", "User Two", "user2"));
        SetupMgmAdmins();

        await _useCase.Execute();

        _mocker.GetMock<ISaveSyncStatus>()
            .Verify(x => x.Execute(
                    It.Is<SaveSyncStatusModel>(m => m.Success && m.SyncedCount == 2)),
                Times.Once);
    }

    private void SetupSyncSettings() =>
        _mocker.GetMock<IGetSyncSettings>()
            .Setup(x => x.Execute())
            .Returns(new GetSyncSettingsModel
            {
                IntervalMinutes = 60,
                ApiUrl = "https://mgm.company.local",
                ApiKey = "key",
                ApiSecret = "$encrypted$"
            });

    private void SetupLdapSettings() =>
        _mocker.GetMock<IGetLdapSettings>()
            .Setup(x => x.Execute())
            .Returns(new GetLdapSettingsModel
            {
                ServerUrl = "10.27.0.80",
                Port = 636,
                UseSsl = true,
                BaseDn = "DC=sambadomain,DC=local",
                ServiceAccountDn = "CN=svc,DC=sambadomain,DC=local",
                BindPassword = "$encrypted$",
                UsernameAttribute = "sAMAccountName",
                EmailAttribute = "mail",
                DisplayNameAttribute = "displayName",
                SyncGroupDn = null
            });

    private void SetupDecrypt() =>
        _mocker.GetMock<IDecryptValue>()
            .Setup(x => x.Execute(It.IsAny<DecryptValueRequest>()))
            .Returns(new DecryptValueResult("plainPassword"));

    private void SetupLdapUsers(params LdapUser[] users) =>
        _mocker.GetMock<IGetUsersFromActiveDirectory>()
            .Setup(x => x.Execute(It.IsAny<LdapSettingsModel>()))
            .Returns(users.ToList());

    private void SetupMgmAdmins(params MgmAdmin[] admins) =>
        _mocker.GetMock<IGetAdminsFromMgm>()
            .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
            .ReturnsAsync(admins.ToList());
}