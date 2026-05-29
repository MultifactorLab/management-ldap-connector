using App.Features.AdminPanel.AdminAccount;

namespace App.SharedPorts;

public interface ISaveAdminAccount
{
    void Execute(SaveAdminAccountModel adminAccount);
}