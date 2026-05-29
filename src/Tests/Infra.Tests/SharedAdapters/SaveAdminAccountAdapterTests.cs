using App.Features.AdminPanel.AdminAccount;
using Infra.Features.AdminPanel.AdminAccount.Models;
using Infra.Integrations.LiteDb;
using Infra.SharedAdapters.SaveAdminAccount;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.SharedAdapters;

public class SaveAdminAccountAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbAdminAccount> _collection;
    private readonly SaveAdminAccountAdapter _adapter;

    public SaveAdminAccountAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbAdminAccount>();

        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<SaveAdminAccountAdapter>();
    }

    [Fact]
    public void Execute_WhenAccountAlreadyExists_UpdateExisting()
    {
        _collection.Insert(new DbAdminAccount
        {
            Username = "admin",
            PasswordHash = "$2a$12$oldhash",
            MustChangePassword = true
        });

        var model = new SaveAdminAccountModel
        {
            Username = "admin",
            PasswordHash = "$2a$12$newhash",
            MustChangePassword = false
        };

        _adapter.Execute(model);

        var saved = _collection.FindById(DbAdminAccount.AdminAccountId);
        Assert.Equal("$2a$12$newhash", saved.PasswordHash);
        Assert.False(saved.MustChangePassword);
        Assert.Equal(1, _collection.Count()); // не создаёт дубль
    }
}