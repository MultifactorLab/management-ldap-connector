using System.Net;
using System.Text.Json;
using App.Features.BackgroundWorker.SyncAdminsToMgm;
using Infra.Features.BackgroundWorker.SyncAdminsToMgm;
using Infra.Integrations.MgmHttpClientFactory;
using Infra.Tests.Features.BackgroundWorker.Helpers;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.BackgroundWorker;

public class MgmSyncAdminsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly MgmSyncAdminsAdapter _sut;
 
    public MgmSyncAdminsAdapterTests()
    {
        _sut = _mocker.CreateInstance<MgmSyncAdminsAdapter>();
    }
 
    [Fact]
    public async Task Execute_WhenUnauthorized_ThrowsUnauthorizedAccessException()
    {
        SetupClientFactory(HttpStatusCode.Unauthorized);
 
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.Execute(BuildAdmins()));
    }
 
    [Fact]
    public async Task Execute_WhenServerError_ThrowsHttpRequestException()
    {
        SetupClientFactory(HttpStatusCode.InternalServerError);
 
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _sut.Execute(BuildAdmins()));
    }
 
    [Fact]
    public async Task Execute_WhenCalled_CreateClientViaFactory()
    {
        SetupClientFactory(HttpStatusCode.OK);

        await _sut.Execute(BuildAdmins());

        _mocker.GetMock<IMgmHttpClientFactory>()
            .Verify(x => x.Create(), Times.Once);
    }
 
    [Fact]
    public async Task Execute_WhenCalled_SerializeAdminsWithCorrectStructure()
    {
        string? capturedBody = null;
        var handler = new CaptureBodyHandler(HttpStatusCode.OK, b => capturedBody = b);
        SetupClientFactoryWithHandler(handler);
 
        await _sut.Execute(new List<AdminDefinition>
        {
            new("user1@company.local", "User One", "user1")
        });
 
        Assert.NotNull(capturedBody);
 
        var doc = JsonDocument.Parse(capturedBody!);
        var newAdmins = doc.RootElement.GetProperty("newAdmins");
        Assert.Equal(1, newAdmins.GetArrayLength());
 
        var first = newAdmins[0];
        Assert.Equal("user1@company.local", first.GetProperty("identity").GetString());
        Assert.Equal("User One", first.GetProperty("name").GetString());
        Assert.Equal("user1", first.GetProperty("ldapUsername").GetString());
    }
 
    private void SetupClientFactory(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://mgm.company.local") };
 
        _mocker.GetMock<IMgmHttpClientFactory>()
            .Setup(x => x.Create())
            .Returns(client);
    }
 
    private void SetupClientFactoryWithHandler(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://mgm.company.local") };
        _mocker.GetMock<IMgmHttpClientFactory>()
            .Setup(x => x.Create())
            .Returns(client);
    }
 
    private static IReadOnlyList<AdminDefinition> BuildAdmins() =>
        new List<AdminDefinition>
        {
            new("user1@company.local", "User One", "user1")
        };
}