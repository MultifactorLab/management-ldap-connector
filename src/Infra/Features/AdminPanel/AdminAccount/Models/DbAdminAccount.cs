using App.Features.AdminPanel.AdminAccount;

namespace Infra.Features.AdminPanel.AdminAccount.Models;

internal sealed class DbAdminAccount
{
    public const string AdminAccountId = "admin-account";
    public string Id { get; private set; } = AdminAccountId;
    public required string Username { get; set; } = "admin";

    /// <summary>Хранится как bcrypt-хеш</summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// true при первом запуске — пользователь обязан сменить пароль.
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    public static GetAdminAccountModel ToAppModel(DbAdminAccount dbModel)
    {
        return new GetAdminAccountModel()
        {
            Username = dbModel.Username,
            PasswordHash = dbModel.PasswordHash,
            MustChangePassword = dbModel.MustChangePassword
        };
    }
    
    public static DbAdminAccount FromAppModel(SaveAdminAccountModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new DbAdminAccount
        {
            Username = model.Username,
            PasswordHash = model.PasswordHash,
            MustChangePassword = model.MustChangePassword
        };
    }
}