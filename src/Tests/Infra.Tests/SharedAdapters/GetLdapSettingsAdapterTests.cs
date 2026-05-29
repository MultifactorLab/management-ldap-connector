using Infra.Features.AdminPanel.LdapSettings.Models;
using Infra.Integrations.LiteDb;
using Infra.SharedAdapters.GetLdapSettings;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.SharedAdapters;

public class GetLdapSettingsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbLdapSettings> _collection;
    private readonly GetLdapSettingsAdapter _adapter;

    public GetLdapSettingsAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbLdapSettings>();

        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<GetLdapSettingsAdapter>();
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
        _collection.Insert(new DbLdapSettings
        {
            ServerUrl = "ldap://company.local",
            Port = 389,
            UseSsl = false,
            BaseDn = "DC=company,DC=local",
            ServiceAccountDn = "CN=svc,DC=company,DC=local",
            BindPassword = "encryptedpassword",
            UsernameAttribute = "sAMAccountName",
            EmailAttribute = "mail",
            DisplayNameAttribute = "displayName",
            SyncGroupDn = null
        });

        var result = _adapter.Execute();

        Assert.NotNull(result);
        Assert.Equal("ldap://company.local", result.ServerUrl);
        Assert.Equal(389, result.Port);
        Assert.Equal("DC=company,DC=local", result.BaseDn);
    }
}