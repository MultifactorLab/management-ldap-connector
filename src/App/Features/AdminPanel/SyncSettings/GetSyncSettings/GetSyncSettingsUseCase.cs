using App.Features.AdminPanel.SyncSettings.GetSyncSettings.Models;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;

namespace App.Features.AdminPanel.SyncSettings.GetSyncSettings;

public interface IGetSyncSettingsUseCase
{
    GetSyncSettingsResult? Execute();
}
 
internal sealed class GetSyncSettingsUseCase : IGetSyncSettingsUseCase
{
    private readonly IGetSyncSettings _getSyncSettings;
    private readonly IDecryptValue _decryptValue;

    public GetSyncSettingsUseCase(IGetSyncSettings getSyncSettings,
        IDecryptValue decryptValue)
    {
        _getSyncSettings = getSyncSettings;
        _decryptValue = decryptValue;
    }
 
    public GetSyncSettingsResult? Execute()
    {
        var settings = _getSyncSettings.Execute();
        if (settings is null)
        {
            return null;
        }

        var decryptResult = _decryptValue.Execute(new DecryptValueRequest(settings.ApiSecret));

        var getSettingsModel = settings with
        {
            ApiSecret = decryptResult.PlainText
        };
        
        return new GetSyncSettingsResult(getSettingsModel);;
    }
}

public sealed record GetSyncSettingsResult(GetSyncSettingsModel Settings);