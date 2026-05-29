using App.SharedPorts.HashPassword;
using Infra.Features.AdminPanel.AdminAccount;
using Infra.Features.AdminPanel.AdminAccount.Models;
using Infra.Integrations.LiteDb;
using Infra.SharedAdapters.GetAdminAccount;
using LiteDB;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Infra.Tests.SharedAdapters;

public class GetAdminAccountAdapterTests
{
    private const string DefaultPassword = "test_default_password";
 
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbAdminAccount> _collection;
    private readonly GetAdminAccountAdapter _adapter;
 
    public GetAdminAccountAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbAdminAccount>();
 
        var config = Options.Create(new AdminConfig 
        { 
            DefaultPassword = DefaultPassword
        });
 
        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _mocker.Use(config);
        _adapter = _mocker.CreateInstance<GetAdminAccountAdapter>();
    }
 
    [Fact]
    public void Execute_WhenAccountExists_ReturnAccount()
    {
        var dbAccount = new DbAdminAccount
        {
            Username = "admin",
            PasswordHash = "$2a$12$hash",
            MustChangePassword = false
        };
        _collection.Insert(dbAccount);
 
        var result = _adapter.Execute();
 
        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("$2a$12$hash", result.PasswordHash);
        Assert.False(result.MustChangePassword);
    }
 
    [Fact]
    public void Execute_WhenAccountDoesNotExist_SeedDefaultAdminAndReturn()
    {
        const string expectedHash = "$2a$12$seeded_hash";
 
        _mocker.GetMock<IHashPassword>()
            .Setup(x => x.Execute(It.Is<HashPasswordRequest>(
                r => r.Password == DefaultPassword)))
            .Returns(new HashPasswordResult(expectedHash));
 
        var result = _adapter.Execute();
 
        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal(expectedHash, result.PasswordHash);
        Assert.True(result.MustChangePassword);
    }
 
    [Fact]
    public void Execute_WhenAccountDoesNotExist_SeedAdminToDatabase()
    {
        _mocker.GetMock<IHashPassword>()
            .Setup(x => x.Execute(It.IsAny<HashPasswordRequest>()))
            .Returns(new HashPasswordResult("$2a$12$hash"));
 
        _adapter.Execute();
 
        var saved = _collection.FindById(DbAdminAccount.AdminAccountId);
        Assert.NotNull(saved);
        Assert.Equal("admin", saved.Username);
    }
 
    [Fact]
    public void Execute_CalledTwiceOnEmptyDb_DoesNotCreateDuplicates()
    {
        _mocker.GetMock<IHashPassword>()
            .Setup(x => x.Execute(It.IsAny<HashPasswordRequest>()))
            .Returns(new HashPasswordResult("$2a$12$hash"));
 
        _adapter.Execute();
        _adapter.Execute();
 
        Assert.Equal(1, _collection.Count());
    }
}