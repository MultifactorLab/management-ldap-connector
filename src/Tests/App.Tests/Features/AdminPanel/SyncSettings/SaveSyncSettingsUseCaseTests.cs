using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;
using App.SharedPorts.Encryption.EncryptValue;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.AdminPanel.SyncSettings;

public class SaveSyncSettingsUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ISaveSyncSettingsUseCase _useCase;
 
    public SaveSyncSettingsUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<SaveSyncSettingsUseCase>();
    }
 
    [Fact]
    public void Execute_EncryptApiSecretBeforeSaving()
    {
        SetupEncrypt("encryptedSecret");
 
        _useCase.Execute(new SaveSyncSettingsRequest(BuildSettings(apiSecret: "plainSecret")));
 
        _mocker.GetMock<ISaveSyncSettings>()
            .Verify(x => x.Execute(
                It.Is<SaveSyncSettingsModel>(s => s.ApiSecret == "encryptedSecret")),
                Times.Once);
    }
 
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Execute_WhenIntervalIsZeroOrNegative_ReturnFailure(int interval)
    {
        var result = _useCase.Execute(new SaveSyncSettingsRequest(BuildSettings(intervalMinutes: interval)));
 
        Assert.False(result.IsSuccess);
        Assert.Equal(SaveSyncSettingsError.InvalidIntervalMinutes, result.Error);
    }
 
    private void SetupEncrypt(string cipherText = "encrypted") =>
        _mocker.GetMock<IEncryptValue>()
            .Setup(x => x.Execute(It.IsAny<EncryptValueRequest>()))
            .Returns(new EncryptValueResult(cipherText));
 
    private static SaveSyncSettingsModel BuildSettings(
        string apiSecret = "plainSecret",
        int intervalMinutes = 60) =>
        new()
        {
            IntervalMinutes = intervalMinutes,
            ApiUrl = "https://mgm.company.local",
            ApiKey = "key",
            ApiSecret = apiSecret
        };
}
 