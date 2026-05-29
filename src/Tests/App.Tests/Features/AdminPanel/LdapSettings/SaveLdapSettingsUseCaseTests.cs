using App.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;
using App.SharedPorts.Encryption.EncryptValue;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.AdminPanel.LdapSettings;

public class SaveLdapSettingsUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ISaveLdapSettingsUseCase _useCase;
 
    public SaveLdapSettingsUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<SaveLdapSettingsUseCase>();
    }
 
    [Fact]
    public void Execute_EncryptBindPasswordBeforeSaving()
    {
        SetupEncrypt("encryptedPassword");
 
        _useCase.Execute(new SaveLdapSettingsRequest(BuildSettings(bindPassword: "plainPassword")));
 
        _mocker.GetMock<ISaveLdapSettings>()
            .Verify(x => x.Execute(
                It.Is<SaveLdapSettingsModel>(s => s.BindPassword == "encryptedPassword")),
                Times.Once);
    }
 
    [Fact]
    public void Execute_DoesNotSavePlainPassword()
    {
        SetupEncrypt("encryptedPassword");
 
        _useCase.Execute(new SaveLdapSettingsRequest(BuildSettings(bindPassword: "plainPassword")));
 
        _mocker.GetMock<ISaveLdapSettings>()
            .Verify(x => x.Execute(
                It.Is<SaveLdapSettingsModel>(s => s.BindPassword == "plainPassword")),
                Times.Never);
    }
 
    [Theory]
    [InlineData("invalid-dn")]
    [InlineData("CN")]
    [InlineData("=value")]
    [InlineData("CN=,DC=local")]
    public void Execute_WhenSyncGroupDnIsInvalid_ReturnFailure(string invalidDn)
    {
        var result = _useCase.Execute(new SaveLdapSettingsRequest(BuildSettings(syncGroupDn: invalidDn)));
 
        Assert.False(result.IsSuccess);
        Assert.Equal(SaveLdapSettingsError.InvalidSyncGroupDn, result.Error);
    }
 
    private void SetupEncrypt(string cipherText) =>
        _mocker.GetMock<IEncryptValue>()
            .Setup(x => x.Execute(It.IsAny<EncryptValueRequest>()))
            .Returns(new EncryptValueResult(cipherText));
 
    private static SaveLdapSettingsModel BuildSettings(
        string bindPassword = "plainPassword",
        string? syncGroupDn = null) =>
        new()
        {
            ServerUrl = "ldap://company.local",
            Port = 389,
            UseSsl = false,
            BaseDn = "DC=company,DC=local",
            ServiceAccountDn = "CN=svc,DC=company,DC=local",
            BindPassword = bindPassword,
            UsernameAttribute = "sAMAccountName",
            EmailAttribute = "mail",
            DisplayNameAttribute = "displayName",
            SyncGroupDn = syncGroupDn
        };
}