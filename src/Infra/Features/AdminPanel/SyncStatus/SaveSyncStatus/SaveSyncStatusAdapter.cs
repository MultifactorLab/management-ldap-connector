using App.Features.AdminPanel.SyncStatus.SaveSyncStatus;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;
using Infra.Features.AdminPanel.SyncStatus.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.Features.AdminPanel.SyncStatus.SaveSyncStatus;

internal class SaveSyncStatusAdapter : ISaveSyncStatus
{
    private readonly ILiteCollection<DbSyncStatus> _collection;

    public SaveSyncStatusAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbSyncStatus>();
    }

    public void Execute(SaveSyncStatusModel syncStatus)
    {
        var dbSetting = DbSyncStatus.FromAppModel(syncStatus);

        _collection.Upsert(dbSetting);
    }
}