using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;

namespace App.Features.AdminPanel.LdapSettings.TestLdapConnection;

public interface ITestLdapConnection
{
    TestLdapConnectionResult Execute(TestLdapConnectionModel testConnectionModel);
}