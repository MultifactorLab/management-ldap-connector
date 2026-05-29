namespace App.Features.AdminPanel.AdminAccount;

public sealed record SaveAdminAccountModel
{
    public required string Username { get; init; }

    /// <summary>Хранится как bcrypt-хеш</summary>
    public required string PasswordHash { get; init; }

    /// <summary>
    /// true при первом запуске — пользователь обязан сменить пароль.
    /// </summary>
    public bool MustChangePassword { get; init; }
}