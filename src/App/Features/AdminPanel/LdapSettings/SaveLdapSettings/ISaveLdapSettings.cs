using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;

namespace App.Features.AdminPanel.LdapSettings.SaveLdapSettings;

public interface ISaveLdapSettings
{
    void Execute(SaveLdapSettingsModel ldapSettings);
}