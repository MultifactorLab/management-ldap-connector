using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;

namespace App.Features.AdminPanel.SyncSettings.SaveSyncSettings;

public interface ISaveSyncSettings
{
    void Execute(SaveSyncSettingsModel syncSettings);
}