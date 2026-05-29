using App.SharedPorts;
using App.SharedPorts.HashPassword;

namespace App.Features.AdminPanel.AdminAccount.ChangeAdminPassword;

public interface IChangeAdminPasswordUseCase
{
    void Execute(ChangeAdminPasswordModel request);
}

internal sealed class ChangeAdminPasswordUseCase : IChangeAdminPasswordUseCase
{
    private readonly IGetAdminAccount _getAdminAccount;
    private readonly ISaveAdminAccount _saveAdminAccount;
    private readonly IHashPassword _hashPassword;

    public ChangeAdminPasswordUseCase(
        IGetAdminAccount getAdminAccount,
        ISaveAdminAccount saveAdminAccount,
        IHashPassword hashPassword)
    {
        _getAdminAccount = getAdminAccount;
        _saveAdminAccount = saveAdminAccount;
        _hashPassword = hashPassword;
    }

    public void Execute(ChangeAdminPasswordModel request)
    {
        var account = _getAdminAccount.Execute();

        var hashResult = _hashPassword.Execute(new HashPasswordRequest(request.NewPassword));

        var saveAdminAccount = new SaveAdminAccountModel
        {
            Username = account.Username,
            PasswordHash = hashResult.Hash,
            MustChangePassword = false
        };

        _saveAdminAccount.Execute(saveAdminAccount);
    }
}

public sealed record ChangeAdminPasswordModel(string NewPassword);