using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;
using Infra.Features.AdminPanel.LdapSettings.Models;
using Infra.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using Infra.Integrations.LiteDb;
using LiteDB;
using Moq.AutoMock;

namespace Infra.Tests.Features.AdminPanel.LdapSettings;

public class SaveLdapSettingsAdapterTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILiteCollection<DbLdapSettings> _collection;
    private readonly SaveLdapSettingsAdapter _adapter;

    public SaveLdapSettingsAdapterTests()
    {
        var db = new LiteDatabase(":memory:");
        _collection = db.GetCollection<DbLdapSettings>();

        _mocker.GetMock<ILiteDbConnection>()
            .Setup(x => x.Database)
            .Returns(db);
        _adapter = _mocker.CreateInstance<SaveLdapSettingsAdapter>();
    }

    [Fact]
    public void Execute_WhenSettingsNew_InsertIntoDatabase()
    {
        var model = BuildModel();

        _adapter.Execute(model);

        var saved = _collection.FindById(DbLdapSettings.LdapSettingsId);
        Assert.NotNull(saved);
        Assert.Equal("ldap://company.local", saved.ServerUrl);
    }

    [Fact]
    public void Execute_WhenSettingsAlreadyExist_UpdateExisting()
    {
        _adapter.Execute(BuildModel("ldap://old.local"));
        _adapter.Execute(BuildModel("ldap://new.local"));

        var saved = _collection.FindById(DbLdapSettings.LdapSettingsId);
        Assert.Equal("ldap://new.local", saved.ServerUrl);
        Assert.Equal(1, _collection.Count());
    }

    private static SaveLdapSettingsModel BuildModel(string serverUrl = "ldap://company.local",
        string? syncGroupDn = null) =>
        new()
        {
            ServerUrl = serverUrl,
            Port = 389,
            UseSsl = false,
            BaseDn = "DC=company,DC=local",
            ServiceAccountDn = "CN=svc,DC=company,DC=local",
            BindPassword = "encryptedpassword",
            UsernameAttribute = "sAMAccountName",
            EmailAttribute = "mail",
            DisplayNameAttribute = "displayName",
            SyncGroupDn = syncGroupDn
        };
}