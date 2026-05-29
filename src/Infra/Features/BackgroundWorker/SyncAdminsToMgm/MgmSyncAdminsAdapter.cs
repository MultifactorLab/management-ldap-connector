using System.Net;
using System.Text;
using System.Text.Json;
using App.Features.BackgroundWorker.SyncAdminsToMgm;
using Infra.Integrations.MgmHttpClientFactory;

namespace Infra.Features.BackgroundWorker.SyncAdminsToMgm;

internal sealed class MgmSyncAdminsAdapter : ISyncAdminsToMgm
{
    private readonly IMgmHttpClientFactory _clientFactory;
    private const string EndpointName = "v1/ldap/admins";

    public MgmSyncAdminsAdapter(IMgmHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task Execute(IReadOnlyList<AdminDefinition> admins, CancellationToken ct = default)
    {
        var client = _clientFactory.Create();

        var payload = new
        {
            newAdmins = admins.Select(a => new
            {
                identity = a.Email,
                name = a.Name,
                ldapUsername = a.LdapUsername
            })
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(EndpointName, content, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("MGM API returned 401. Check ApiKey and ApiSecret.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"MGM API returned {(int)response.StatusCode} for POST {EndpointName}.");
        }
    }
}