using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;

namespace App.Features.AdminPanel.LdapSettings.GetLdapSettings;

public interface IGetLdapSettingsUseCase
{
    GetLdapSettingsResult? Execute();
}

internal sealed class GetLdapSettingsUseCase : IGetLdapSettingsUseCase
{
    private readonly IGetLdapSettings _getLdapSettings;
    private readonly IDecryptValue _decryptValue;

    public GetLdapSettingsUseCase(IGetLdapSettings getLdapSettings,
        IDecryptValue decryptValue)
    {
        _getLdapSettings = getLdapSettings;
        _decryptValue = decryptValue;
    }

    public GetLdapSettingsResult? Execute()
    {
        var settings = _getLdapSettings.Execute();
        if (settings is null)
        {
            return null;
        }

        var decryptResult = _decryptValue.Execute(new DecryptValueRequest(settings.BindPassword));

        var getSettingsModel = settings with
        {
            BindPassword = decryptResult.PlainText
        };

        return new GetLdapSettingsResult(getSettingsModel);
    }
}

public sealed record GetLdapSettingsResult(GetLdapSettingsModel Settings);