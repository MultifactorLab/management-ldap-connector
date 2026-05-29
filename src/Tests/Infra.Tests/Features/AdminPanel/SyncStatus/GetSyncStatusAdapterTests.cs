using Infra.Features.AdminPanel.SyncStatus.GetSyncStatus;
using Infra.Features.AdminPanel.SyncStatus.Models;
using Infra.Integrations.LiteDb;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.Features.AdminPanel.SyncStatus;

public class GetSyncStatusAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbSyncStatus> _collection;
    private readonly GetSyncStatusAdapter _adapter;
 
    public GetSyncStatusAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbSyncStatus>();
 
        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<GetSyncStatusAdapter>();
    }
 
    [Fact]
    public void Execute_WhenStatusDoesNotExist_ReturnNull()
    {
        var result = _adapter.Execute();
 
        Assert.Null(result);
    }
 
    [Fact]
    public void Execute_WhenStatusExists_ReturnStatus()
    {
        var lastRun = DateTime.Now.AddHours(-1);
        _collection.Insert(new DbSyncStatus
        {
            LastRunAt = lastRun,
            SyncedCount = 42,
            Success = true,
            ErrorMessage = null
        });
 
        var result = _adapter.Execute();
 
        Assert.NotNull(result);
        Assert.Equal(42, result.SyncedCount);
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }
 
    [Fact]
    public void Execute_WhenStatusHasError_ReturnErrorMessage()
    {
        _collection.Insert(new DbSyncStatus
        {
            LastRunAt = DateTime.Now,
            SyncedCount = 0,
            Success = false,
            ErrorMessage = "Connection refused"
        });
 
        var result = _adapter.Execute();
 
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Connection refused", result.ErrorMessage);
    }
}