using App.Features.AdminPanel.AdminAccount;
using App.SharedPorts;
using App.SharedPorts.HashPassword;
using Infra.Features.AdminPanel.AdminAccount;
using Infra.Features.AdminPanel.AdminAccount.Models;
using Infra.Integrations.LiteDb;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Infra.SharedAdapters.GetAdminAccount;

internal sealed class GetAdminAccountAdapter : IGetAdminAccount
{
    private readonly ILiteCollection<DbAdminAccount> _collection;
    private readonly IHashPassword _hashPassword;
    private readonly string _defaultPassword;

    public GetAdminAccountAdapter(
        ILiteDbConnection connection,
        IHashPassword hashPassword,
        IOptions<AdminConfig> options)
    {
        _collection = connection.Database.GetCollection<DbAdminAccount>();
        _hashPassword = hashPassword;
        _defaultPassword = options.Value.DefaultPassword;
    }

    public GetAdminAccountModel Execute()
    {
        var adminAccount = _collection.FindById(DbAdminAccount.AdminAccountId);

        if (adminAccount is null)
        {
            adminAccount = SeedDefaultAdmin();
        }

        return DbAdminAccount.ToAppModel(adminAccount);
    }

    private DbAdminAccount SeedDefaultAdmin()
    {
        var hashResult = _hashPassword.Execute(new HashPasswordRequest(_defaultPassword));

        var dbAdminAccount = new DbAdminAccount
        {
            Username = "admin",
            PasswordHash = hashResult.Hash,
            MustChangePassword = true
        };

        _collection.Insert(dbAdminAccount);

        return dbAdminAccount;
    }
}