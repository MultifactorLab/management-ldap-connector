using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;
using App.SharedPorts.Encryption.EncryptValue;
using Microsoft.Extensions.Logging;

namespace App.Features.AdminPanel.LdapSettings.SaveLdapSettings;

public interface ISaveLdapSettingsUseCase
{
    SaveLdapSettingsResult Execute(SaveLdapSettingsRequest request);
}

internal sealed class SaveLdapSettingsUseCase : ISaveLdapSettingsUseCase
{
    private readonly ISaveLdapSettings _saveLdapSettings;
    private readonly IEncryptValue _encryptValue;
    private readonly ILogger<SaveLdapSettingsUseCase> _logger;

    public SaveLdapSettingsUseCase(
        ISaveLdapSettings saveLdapSettings,
        IEncryptValue encryptValue,
        ILogger<SaveLdapSettingsUseCase> logger)
    {
        _saveLdapSettings = saveLdapSettings;
        _encryptValue = encryptValue;
        _logger = logger;
    }

    public SaveLdapSettingsResult Execute(SaveLdapSettingsRequest request)
    {
        if (!IsValidDn(request.Settings.SyncGroupDn))
        {
            _logger.LogWarning("Save LDAP settings failed: invalid SyncGroupDn {SyncGroupDn}.",
                request.Settings.SyncGroupDn);
            return SaveLdapSettingsResult.Failure(SaveLdapSettingsError.InvalidSyncGroupDn);
        }

        var encryptResult = _encryptValue.Execute(new EncryptValueRequest(request.Settings.BindPassword));

        var settingsToSave = request.Settings with
        {
            BindPassword = encryptResult.CipherText,
            SyncGroupDn = string.IsNullOrWhiteSpace(request.Settings.SyncGroupDn)
                ? null
                : request.Settings.SyncGroupDn.Trim()
        };

        _saveLdapSettings.Execute(settingsToSave);

        _logger.LogInformation("LDAP settings saved successfully.");

        return SaveLdapSettingsResult.Success();
    }

    private static bool IsValidDn(string? dn)
    {
        if (string.IsNullOrWhiteSpace(dn))
        {
            return true;
        }

        var parts = dn.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        return parts.All(part =>
        {
            var eqIndex = part.IndexOf('=');
            return eqIndex > 0 && eqIndex < part.Length - 1;
        });
    }
}

public sealed record SaveLdapSettingsRequest(SaveLdapSettingsModel Settings);

public enum SaveLdapSettingsError
{
    InvalidSyncGroupDn
}

public sealed record SaveLdapSettingsResult
{
    public bool IsSuccess { get; init; }
    public SaveLdapSettingsError? Error { get; init; }

    public static SaveLdapSettingsResult Success() => new() { IsSuccess = true };

    public static SaveLdapSettingsResult Failure(SaveLdapSettingsError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}