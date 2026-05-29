using System.Net.Http.Headers;
using System.Text;
using App.Features.AdminPanel.SyncSettings.GetSyncSettings;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;

namespace Infra.Integrations.MgmHttpClientFactory;

internal interface IMgmHttpClientFactory
{
    HttpClient Create();
}

internal class MgmHttpClientFactory : IMgmHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGetSyncSettings _getSyncSettings;
    private readonly IDecryptValue _decryptValue;
 
    public MgmHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        IGetSyncSettings getSyncSettings,
        IDecryptValue decryptValue)
    {
        _httpClientFactory = httpClientFactory;
        _getSyncSettings = getSyncSettings;
        _decryptValue = decryptValue;
    }
 
    public HttpClient Create()
    {
        var settings = _getSyncSettings.Execute()
                       ?? throw new InvalidOperationException("Sync settings not configured.");
 
        var decryptResult = _decryptValue.Execute(
            new DecryptValueRequest(settings.ApiSecret));
 
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(settings.ApiUrl);
        client.DefaultRequestHeaders.Authorization =
            BuildBasicAuth(settings.ApiKey, decryptResult.PlainText);
 
        return client;
    }
 
    private static AuthenticationHeaderValue BuildBasicAuth(string apiKey, string apiSecret)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
        return new AuthenticationHeaderValue("Basic", credentials);
    }
}