using System.Net;
using System.Text.Json;
using App.Features.BackgroundWorker.GetAdminsFromMgm;
using Infra.Integrations.MgmHttpClientFactory;

namespace Infra.Features.BackgroundWorker.GetAdminsFromMgm;

internal sealed class MgmGetAdminsAdapter : IGetAdminsFromMgm
{
    private readonly IMgmHttpClientFactory _clientFactory;
    private const string EndpointName = "v1/ldap/admins";

    public MgmGetAdminsAdapter(IMgmHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IReadOnlyList<MgmAdmin>> Execute(CancellationToken ct = default)
    {
        var client = _clientFactory.Create();

        var response = await client.GetAsync(EndpointName, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "MGM API returned 401. Check ApiKey and ApiSecret.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"MGM API returned {(int)response.StatusCode} for GET {EndpointName}.");
        }

        var json = await response.Content.ReadAsStringAsync(ct);

        var mgmAdmins = JsonSerializer.Deserialize<List<MgmAdminDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return mgmAdmins?
            .Select(d => new MgmAdmin(d.Email, d.Name, d.LdapUsername))
            .ToList() ?? [];
    }
}

internal sealed record MgmAdminDto(string Email, string Name, string LdapUsername);