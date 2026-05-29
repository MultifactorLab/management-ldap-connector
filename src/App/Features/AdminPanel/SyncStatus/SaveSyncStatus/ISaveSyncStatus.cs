using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;

namespace App.Features.AdminPanel.SyncStatus.SaveSyncStatus;

public interface ISaveSyncStatus
{
    void Execute(SaveSyncStatusModel syncStatus);
}