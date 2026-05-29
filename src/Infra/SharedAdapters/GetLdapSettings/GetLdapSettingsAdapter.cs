using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;
using App.SharedPorts;
using Infra.Features.AdminPanel.LdapSettings.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.SharedAdapters.GetLdapSettings;

internal sealed class GetLdapSettingsAdapter : IGetLdapSettings
{
    private readonly ILiteCollection<DbLdapSettings> _collection;

    public GetLdapSettingsAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbLdapSettings>();
    }

    public GetLdapSettingsModel? Execute()
    {
        var settings = _collection.FindById(DbLdapSettings.LdapSettingsId);

        if (settings is null)
        {
            return null;
        }

        return DbLdapSettings.ToAppModel(settings);
    }
}