using App.Features.AdminPanel.LdapSettings.GetLdapSettings.Models;

namespace App.SharedPorts;

public interface IGetLdapSettings
{
    GetLdapSettingsModel? Execute();
}