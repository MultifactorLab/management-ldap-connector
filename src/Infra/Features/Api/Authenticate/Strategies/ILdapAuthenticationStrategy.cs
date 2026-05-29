using System.DirectoryServices.Protocols;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using App.Features.Api.Authenticate;
using Infra.SharedAdapters.LdapExecutor;

namespace Infra.Features.Api.Authenticate.Strategies;

internal interface ILdapAuthenticationStrategy
{
    AuthenticateLdapUserResult Authenticate(
        LdapConnection connection,
        LdapSettingsModel settings,
        string username,
        string password,
        ILdapExecutor executor);
}