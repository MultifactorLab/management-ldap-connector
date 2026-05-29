using App.Features.AdminPanel.SyncStatus.GetSyncStatus.Models;

namespace App.Features.AdminPanel.SyncStatus.GetSyncStatus;

public interface IGetSyncStatus
{
    GetSyncStatusModel? Execute();
}