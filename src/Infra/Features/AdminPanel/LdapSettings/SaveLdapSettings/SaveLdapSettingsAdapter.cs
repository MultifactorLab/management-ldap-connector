using App.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;
using Infra.Features.AdminPanel.LdapSettings.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.Features.AdminPanel.LdapSettings.SaveLdapSettings;

internal class SaveLdapSettingsAdapter : ISaveLdapSettings
{
    private readonly ILiteCollection<DbLdapSettings> _collection;

    public SaveLdapSettingsAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbLdapSettings>();
    }
    
    public void Execute(SaveLdapSettingsModel ldapSettings)
    {
        ArgumentNullException.ThrowIfNull(ldapSettings);
        
        var dbSetting = DbLdapSettings.FromAppModel(ldapSettings);
        
        _collection.Upsert(dbSetting);
    }
}