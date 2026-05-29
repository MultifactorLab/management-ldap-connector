using App.Features.AdminPanel.SyncStatus.GetSyncStatus;
using App.Features.AdminPanel.SyncStatus.GetSyncStatus.Models;
using Infra.Features.AdminPanel.SyncStatus.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.Features.AdminPanel.SyncStatus.GetSyncStatus;

internal class GetSyncStatusAdapter : IGetSyncStatus
{
    private readonly ILiteCollection<DbSyncStatus> _collection;

    public GetSyncStatusAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbSyncStatus>();
    }

    public GetSyncStatusModel? Execute()
    {
        var syncStatus = _collection.FindById(DbSyncStatus.SyncStatusId);

        if (syncStatus is null)
        {
            return null;
        }

        return DbSyncStatus.ToAppModel(syncStatus);
    }
}