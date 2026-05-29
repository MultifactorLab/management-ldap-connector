using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;

namespace App.Features.BackgroundWorker.GetUsersFromActiveDirectory;

public interface IGetUsersFromActiveDirectory
{
    IReadOnlyList<LdapUser> Execute(LdapSettingsModel settings);
}

public sealed record LdapUser(
    string Email,
    string DisplayName,
    string Username);
