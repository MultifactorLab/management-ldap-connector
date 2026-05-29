using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;

namespace Infra.Features.AdminPanel.LdapSettings.Models;

internal sealed class DbLdapSettings
{
    public const string LdapSettingsId = "ldap-settings";
    public string Id { get; private set; } = LdapSettingsId;
    public required string ServerUrl { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public required string BaseDn { get; set; }
    public required string ServiceAccountDn { get; set; }

    /// <summary>Хранится зашифрованным через EncryptValueAdapter</summary>
    public required string BindPassword { get; set; }

    /// <summary>Атрибут для поиска пользователя: sAMAccountName / uid</summary>
    public required string UsernameAttribute { get; set; }

    public required string EmailAttribute { get; set; }
    public required string DisplayNameAttribute { get; set; }

    /// <summary>
    /// DN группы для синхронизации, например:
    /// CN=MGM Admins,OU=Groups,DC=company,DC=local
    /// Если null — синхронизируются все пользователи из BaseDn
    /// </summary>
    public string? SyncGroupDn { get; set; }

    public static GetLdapSettingsModel ToAppModel(DbLdapSettings dbModel)
    {
        return new GetLdapSettingsModel
        {
            ServerUrl = dbModel.ServerUrl,
            Port = dbModel.Port,
            UseSsl = dbModel.UseSsl,
            BaseDn = dbModel.BaseDn,
            ServiceAccountDn = dbModel.ServiceAccountDn,
            BindPassword = dbModel.BindPassword,
            UsernameAttribute = dbModel.UsernameAttribute,
            EmailAttribute = dbModel.EmailAttribute,
            DisplayNameAttribute = dbModel.DisplayNameAttribute,
            SyncGroupDn = dbModel.SyncGroupDn,
        };
    }
    
    public static DbLdapSettings FromAppModel(SaveLdapSettingsModel model)
    {
        return new DbLdapSettings
        {
            ServerUrl = model.ServerUrl,
            Port = model.Port,
            UseSsl = model.UseSsl,
            BaseDn = model.BaseDn,
            ServiceAccountDn = model.ServiceAccountDn,
            BindPassword = model.BindPassword,
            UsernameAttribute = model.UsernameAttribute,
            EmailAttribute = model.EmailAttribute,
            DisplayNameAttribute = model.DisplayNameAttribute,
            SyncGroupDn = model.SyncGroupDn,
        };
    }
}