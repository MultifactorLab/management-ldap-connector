using App.Features.AdminPanel.AdminAccount;
using App.SharedPorts;
using Infra.Features.AdminPanel.AdminAccount.Models;
using Infra.Integrations.LiteDb;
using LiteDB;

namespace Infra.SharedAdapters.SaveAdminAccount;

internal class SaveAdminAccountAdapter: ISaveAdminAccount
{
    private readonly ILiteCollection<DbAdminAccount> _collection;

    public SaveAdminAccountAdapter(ILiteDbConnection connection)
    {
        _collection = connection.Database.GetCollection<DbAdminAccount>();
    }
    
    public void Execute(SaveAdminAccountModel adminAccount)
    {
        ArgumentNullException.ThrowIfNull(adminAccount);
        
        var dbAdminAccount = DbAdminAccount.FromAppModel(adminAccount);
        
        _collection.Upsert(dbAdminAccount);
    }
}