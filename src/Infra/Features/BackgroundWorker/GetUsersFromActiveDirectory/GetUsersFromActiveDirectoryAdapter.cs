using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using App.Features.BackgroundWorker.GetUsersFromActiveDirectory;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;

namespace Infra.Features.BackgroundWorker.GetUsersFromActiveDirectory;

internal sealed class GetUsersFromActiveDirectoryAdapter : IGetUsersFromActiveDirectory
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly ILdapExecutor _executor;

    private const string LdapPersonFilter = "(objectClass=person)";
    private const string LdapAllFilter = "(objectClass=*)";
    private const string LdapAttributeName = "member";

    public GetUsersFromActiveDirectoryAdapter(ILdapConnectionFactory connectionFactory,
        ILdapExecutor executor)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
    }

    public IReadOnlyList<LdapUser> Execute(LdapSettingsModel settings)
    {
        using var connection = _connectionFactory.Create(
            new LdapConnectionFactoryModel(
                settings.ServerUrl,
                settings.Port,
                settings.UseSsl));

        _executor.Bind(connection, new NetworkCredential(
            settings.ServiceAccountDn,
            settings.BindPassword));

        return string.IsNullOrWhiteSpace(settings.SyncGroupDn)
            ? GetAllUsers(connection, settings)
            : GetUsersFromGroup(connection, settings);
    }

    private IReadOnlyList<LdapUser> GetAllUsers(LdapConnection connection, LdapSettingsModel settings)
    {
        var searchRequest = new SearchRequest(
            settings.BaseDn,
            LdapPersonFilter,
            SearchScope.Subtree,
            settings.UsernameAttribute,
            settings.EmailAttribute,
            settings.DisplayNameAttribute);

        var response = (SearchResponse)_executor.SendRequest(connection, searchRequest);

        return response.Entries
            .Cast<SearchResultEntry>()
            .Select(entry => MapToLdapUser(entry, settings))
            .Where(user => user is not null)
            .Select(user => user!)
            .ToList();
    }

    private IReadOnlyList<LdapUser> GetUsersFromGroup(LdapConnection connection, LdapSettingsModel settings)
    {
        var groupRequest = new SearchRequest(
            settings.SyncGroupDn,
            LdapAllFilter,
            SearchScope.Base,
            LdapAttributeName);

        var groupResponse = (SearchResponse)_executor.SendRequest(connection, groupRequest);

        if (groupResponse.Entries.Count == 0)
        {
            return [];
        }

        var groupEntry = groupResponse.Entries[0];
        var memberAttribute = groupEntry.Attributes[LdapAttributeName];

        if (memberAttribute is null || memberAttribute.Count == 0)
        {
            return [];
        }

        var users = new List<LdapUser>();

        for (var i = 0; i < memberAttribute.Count; i++)
        {
            var memberDn = GetAttributeValue(memberAttribute, i);

            var userRequest = new SearchRequest(
                memberDn,
                LdapAllFilter,
                SearchScope.Base,
                settings.UsernameAttribute,
                settings.EmailAttribute,
                settings.DisplayNameAttribute);

            var userResponse = (SearchResponse)_executor.SendRequest(connection, userRequest);

            if (userResponse.Entries.Count == 0)
            {
                continue;
            }

            var user = MapToLdapUser(userResponse.Entries[0], settings);
            if (user is not null)
            {
                users.Add(user);
            }
        }

        return users;
    }

    private static LdapUser? MapToLdapUser(SearchResultEntry entry, LdapSettingsModel settings)
    {
        var username = entry.Attributes[settings.UsernameAttribute] is { } usernameAttr
            ? GetAttributeValue(usernameAttr)
            : null;

        var email = entry.Attributes[settings.EmailAttribute] is { } emailAttr
            ? GetAttributeValue(emailAttr)
            : null;

        var displayName = entry.Attributes[settings.DisplayNameAttribute] is { } displayNameAttr
            ? GetAttributeValue(displayNameAttr)
            : null;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return new LdapUser(
            Email: email,
            DisplayName: displayName ?? username,
            Username: username);
    }

    private static string GetAttributeValue(DirectoryAttribute attribute, int index = 0)
    {
        var value = attribute[index];
        return value switch
        {
            string s => s,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => value.ToString() ?? string.Empty
        };
    }
}