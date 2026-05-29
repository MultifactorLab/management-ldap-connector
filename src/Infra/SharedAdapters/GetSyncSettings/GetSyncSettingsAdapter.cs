using App.Features.AdminPanel.SyncSettings.GetSyncSettings.Models;
using App.SharedPorts;
using Infra.Features.AdminPanel.SyncSettings.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.SharedAdapters.GetSyncSettings;

internal class GetSyncSettingsAdapter : IGetSyncSettings
{
    private readonly ILiteCollection<DbSyncSettings> _collection;

    public GetSyncSettingsAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbSyncSettings>();;
    }

    public GetSyncSettingsModel? Execute()
    {
        var syncSettings = _collection.FindById(DbSyncSettings.SyncSettingsId);

        if (syncSettings is null)
        {
            return null;
        }

        return DbSyncSettings.ToAppModel(syncSettings);
    }
}