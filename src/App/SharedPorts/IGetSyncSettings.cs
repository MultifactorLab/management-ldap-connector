using App.Features.AdminPanel.SyncSettings.GetSyncSettings.Models;

namespace App.SharedPorts;

public interface IGetSyncSettings
{
    GetSyncSettingsModel? Execute();
}