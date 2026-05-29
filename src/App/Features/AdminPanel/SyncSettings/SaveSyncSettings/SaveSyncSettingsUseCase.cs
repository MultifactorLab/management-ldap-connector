using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;
using App.SharedPorts.Encryption.EncryptValue;
using Microsoft.Extensions.Logging;

namespace App.Features.AdminPanel.SyncSettings.SaveSyncSettings;

public interface ISaveSyncSettingsUseCase
{
    SaveSyncSettingsResult Execute(SaveSyncSettingsRequest request);
}
 
internal sealed class SaveSyncSettingsUseCase : ISaveSyncSettingsUseCase
{
    private readonly ISaveSyncSettings _saveSyncSettings;
    private readonly IEncryptValue _encryptValue;
    private readonly ISyncSettingsChangeNotifier _changeNotifier;
    private readonly ILogger<SaveSyncSettingsUseCase> _logger;
 
    public SaveSyncSettingsUseCase(
        ISaveSyncSettings saveSyncSettings,
        IEncryptValue encryptValue,
        ISyncSettingsChangeNotifier changeNotifier,
        ILogger<SaveSyncSettingsUseCase> logger)
    {
        _saveSyncSettings = saveSyncSettings;
        _encryptValue = encryptValue;
        _changeNotifier = changeNotifier;
        _logger = logger;
    }
 
    public SaveSyncSettingsResult Execute(SaveSyncSettingsRequest request)
    {
        if (request.Settings.IntervalMinutes <= 0)
        {
            _logger.LogWarning("Save sync settings failed: invalid IntervalMinutes {IntervalMinutes}.",
                request.Settings.IntervalMinutes);
            return SaveSyncSettingsResult.Failure(SaveSyncSettingsError.InvalidIntervalMinutes);
        }
 
        var encryptResult = _encryptValue.Execute(new EncryptValueRequest(request.Settings.ApiSecret));
 
        var settingsToSave = request.Settings with
        {
            ApiSecret = encryptResult.CipherText
        };
 
        _saveSyncSettings.Execute(settingsToSave);
 
        _logger.LogInformation("Sync settings saved successfully.");
 
        _changeNotifier.NotifyChanged();
        
        return SaveSyncSettingsResult.Success();
    }
}

public sealed record SaveSyncSettingsRequest(SaveSyncSettingsModel Settings);

public enum SaveSyncSettingsError
{
    InvalidIntervalMinutes
}
 
public sealed record SaveSyncSettingsResult
{
    public bool IsSuccess { get; init; }
    public SaveSyncSettingsError? Error { get; init; }
 
    public static SaveSyncSettingsResult Success() => new() { IsSuccess = true };
 
    public static SaveSyncSettingsResult Failure(SaveSyncSettingsError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}