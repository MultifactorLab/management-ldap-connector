using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;
using Infra.Features.AdminPanel.SyncSettings.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.Features.AdminPanel.SyncSettings.SaveSyncSettings;

internal class SaveSyncSettingsAdapter : ISaveSyncSettings
{
    private readonly ILiteCollection<DbSyncSettings> _collection;

    public SaveSyncSettingsAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbSyncSettings>();
    }

    public void Execute(SaveSyncSettingsModel syncSettings)
    {
        ArgumentNullException.ThrowIfNull(syncSettings);

        var dbSetting = DbSyncSettings.FromAppModel(syncSettings);

        _collection.Upsert(dbSetting);
    }
}