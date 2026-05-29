using System.Net;
using System.Text;
using System.Text.Json;
using Infra.Features.BackgroundWorker.GetAdminsFromMgm;
using Infra.Integrations.MgmHttpClientFactory;
using Infra.Tests.Features.BackgroundWorker.Helpers;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.Features.BackgroundWorker;

public class MgmGetAdminsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly MgmGetAdminsAdapter _adapter;
 
    public MgmGetAdminsAdapterTests()
    {
        _adapter = _mocker.CreateInstance<MgmGetAdminsAdapter>();
    }
 
    [Fact]
    public async Task Execute_WhenSuccess_ReturnMgmAdminList()
    {
        var dtos = new[]
        {
            new { email = "user1@company.local", name = "User One", ldapUsername = "user1" },
            new { email = "user2@company.local", name = "User Two", ldapUsername = "user2" }
        };
        SetupClientFactory(HttpStatusCode.OK, JsonSerializer.Serialize(dtos));
 
        var result = await _adapter.Execute();
 
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a =>
            a is { Email: "user1@company.local", Name: "User One", LdapUsername: "user1" });
        Assert.Contains(result, a =>
            a is { Email: "user2@company.local", Name: "User Two", LdapUsername: "user2" });
    }
 
    [Fact]
    public async Task Execute_WhenEmptyList_ReturnEmptyList()
    {
        SetupClientFactory(HttpStatusCode.OK, "[]");
 
        var result = await _adapter.Execute();
 
        Assert.Empty(result);
    }
 
    [Fact]
    public async Task Execute_WhenUnauthorized_ThrowsUnauthorizedAccessException()
    {
        SetupClientFactory(HttpStatusCode.Unauthorized, string.Empty);
 
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _adapter.Execute());
    }
 
    [Fact]
    public async Task Execute_WhenServerError_ThrowsHttpRequestException()
    {
        SetupClientFactory(HttpStatusCode.InternalServerError, string.Empty);
 
        await Assert.ThrowsAsync<HttpRequestException>(() => _adapter.Execute());
    }
 
    [Fact]
    public async Task Execute_WhenCalled_CreateClientViaFactory()
    {
        SetupClientFactory(HttpStatusCode.OK, JsonSerializer.Serialize(Array.Empty<object>()));

        await _adapter.Execute();

        _mocker.GetMock<IMgmHttpClientFactory>()
            .Verify(x => x.Create(), Times.Once);
    }
 
    private void SetupClientFactory(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://mgm.company.local") };
 
        _mocker.GetMock<IMgmHttpClientFactory>()
            .Setup(x => x.Create())
            .Returns(client);
    }
}