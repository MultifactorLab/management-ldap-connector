using Infra.Features.AdminPanel.SyncSettings.Models;
using Infra.Integrations.LiteDb;
using Infra.SharedAdapters.GetSyncSettings;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.SharedAdapters;

public class GetSyncSettingsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbSyncSettings> _collection;
    private readonly GetSyncSettingsAdapter _adapter;
 
    public GetSyncSettingsAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbSyncSettings>();
 
        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<GetSyncSettingsAdapter>();
    }
 
    [Fact]
    public void Execute_WhenSettingsDoNotExist_ReturnNull()
    {
        var result = _adapter.Execute();
 
        Assert.Null(result);
    }
 
    [Fact]
    public void Execute_WhenSettingsExist_ReturnSettings()
    {
        _collection.Insert(new DbSyncSettings
        {
            IntervalMinutes = 60,
            ApiUrl = "https://mgm.company.local",
            ApiKey = "key",
            ApiSecret = "encryptedsecret"
        });
 
        var result = _adapter.Execute();
 
        Assert.NotNull(result);
        Assert.Equal(60, result.IntervalMinutes);
        Assert.Equal("https://mgm.company.local", result.ApiUrl);
    }
}