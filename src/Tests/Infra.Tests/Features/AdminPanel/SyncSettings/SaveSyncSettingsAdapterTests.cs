using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;
using Infra.Features.AdminPanel.SyncSettings.Models;
using Infra.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using Infra.Integrations.LiteDb;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.Features.AdminPanel.SyncSettings;

public class SaveSyncSettingsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbSyncSettings> _collection;
    private readonly SaveSyncSettingsAdapter _adapter;

    public SaveSyncSettingsAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbSyncSettings>();

        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<SaveSyncSettingsAdapter>();
    }

    [Fact]
    public void Execute_WhenSettingsNew_InsertIntoDatabase()
    {
        var model = BuildModel();

        _adapter.Execute(model);

        var saved = _collection.FindById(DbSyncSettings.SyncSettingsId);
        Assert.NotNull(saved);
        Assert.Equal("https://mgm.company.local", saved.ApiUrl);
    }

    [Fact]
    public void Execute_WhenSettingsAlreadyExist_UpdateExistingRecord()
    {
        _adapter.Execute(BuildModel(intervalMinutes: 30));
        _adapter.Execute(BuildModel(intervalMinutes: 120));

        var saved = _collection.FindById(DbSyncSettings.SyncSettingsId);
        Assert.Equal(120, saved.IntervalMinutes);
        Assert.Equal(1, _collection.Count());
    }

    private static SaveSyncSettingsModel BuildModel(
        int intervalMinutes = 60) =>
        new()
        {
            IntervalMinutes = intervalMinutes,
            ApiUrl = "https://mgm.company.local",
            ApiKey = "key",
            ApiSecret = "encryptedsecret"
        };
}