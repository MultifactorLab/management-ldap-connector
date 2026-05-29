using System.DirectoryServices.Protocols;
using System.Net;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using App.Features.Api.Authenticate;
using Infra.SharedAdapters.LdapExecutor;

namespace Infra.Features.Api.Authenticate.Strategies;

/// <summary>
/// Стратегия аутентификации для Samba
/// Использует фильтр memberOf для проверки членства в группе синхронизации.
/// </summary>
internal sealed class SambaAuthenticationStrategy : ILdapAuthenticationStrategy
{
    public AuthenticateLdapUserResult Authenticate(
        LdapConnection connection,
        LdapSettingsModel settings,
        string username,
        string password,
        ILdapExecutor executor)
    {
        var searchFilter = BuildSearchFilter(settings, username);
 
        var searchRequest = new SearchRequest(
            settings.BaseDn,
            searchFilter,
            SearchScope.Subtree,
            settings.DisplayNameAttribute);
        
        var searchResponse = (SearchResponse)executor.SendRequest(connection, searchRequest);

        if (searchResponse.Entries.Count == 0)
        {
            return AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.UserNotFound);
        }
 
        var userEntry = searchResponse.Entries[0];
        var userDn = userEntry.DistinguishedName;
 
        try
        {
            executor.Bind(connection, new NetworkCredential(userDn, password));
        }
        catch (LdapException ex) when (ex.ErrorCode == LdapErrorCodes.InvalidCredentials)
        {
            return AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.InvalidCredentials);
        }
 
        var displayName = userEntry.Attributes[settings.DisplayNameAttribute]?[0]?.ToString()
            ?? username;
 
        return AuthenticateLdapUserResult.Success(displayName);
    }
 
    private static string BuildSearchFilter(LdapSettingsModel settings, string username)
    {
        var userFilter = $"({settings.UsernameAttribute}={EscapeLdapFilter(username)})";

        if (string.IsNullOrWhiteSpace(settings.SyncGroupDn))
        {
            return userFilter;
        }
        
        return $"(&{userFilter}(memberOf={EscapeLdapFilter(settings.SyncGroupDn)}))";
    }
 
    private static string EscapeLdapFilter(string value) =>
        value
            .Replace("\\", "\\5c")
            .Replace("*",  "\\2a")
            .Replace("(",  "\\28")
            .Replace(")",  "\\29")
            .Replace("\0", "\\00");
}