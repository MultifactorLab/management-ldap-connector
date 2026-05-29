using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;
using Infra.Features.AdminPanel.SyncStatus.Models;
using Infra.Features.AdminPanel.SyncStatus.SaveSyncStatus;
using Infra.Integrations.LiteDb;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.Features.AdminPanel.SyncStatus;

public class SaveSyncStatusAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbSyncStatus> _collection;
    private readonly SaveSyncStatusAdapter _adapter;

    public SaveSyncStatusAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbSyncStatus>();

        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<SaveSyncStatusAdapter>();
    }

    [Fact]
    public void Execute_WhenStatusIsNew_InsertIntoDatabase()
    {
        var model = new SaveSyncStatusModel
        {
            LastRunAt = DateTime.Now,
            SyncedCount = 10,
            Success = true,
            ErrorMessage = null
        };

        _adapter.Execute(model);

        var saved = _collection.FindById(DbSyncStatus.SyncStatusId);
        Assert.NotNull(saved);
        Assert.Equal(10, saved.SyncedCount);
        Assert.True(saved.Success);
    }

    [Fact]
    public void Execute_WhenStatusAlreadyExists_UpdateExisting()
    {
        _adapter.Execute(new SaveSyncStatusModel
        {
            LastRunAt = DateTime.Now,
            SyncedCount = 5,
            Success = true,
            ErrorMessage = null
        });
        _adapter.Execute(new SaveSyncStatusModel
        {
            LastRunAt = DateTime.Now,
            SyncedCount = 20,
            Success = true,
            ErrorMessage = null
        });

        var saved = _collection.FindById(DbSyncStatus.SyncStatusId);
        Assert.Equal(20, saved.SyncedCount);
        Assert.Equal(1, _collection.Count());
    }

    [Fact]
    public void Execute_WhenSyncFailed_SaveErrorMessage()
    {
        var model = new SaveSyncStatusModel
        {
            LastRunAt = DateTime.Now,
            SyncedCount = 0,
            Success = false,
            ErrorMessage = "LDAP unavailable"
        };

        _adapter.Execute(model);

        var saved = _collection.FindById(DbSyncStatus.SyncStatusId);
        Assert.False(saved.Success);
        Assert.Equal("LDAP unavailable", saved.ErrorMessage);
    }
}