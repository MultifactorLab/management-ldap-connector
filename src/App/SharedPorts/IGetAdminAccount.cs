using App.Features.AdminPanel.AdminAccount;

namespace App.SharedPorts;

public interface IGetAdminAccount
{
    GetAdminAccountModel Execute();
}