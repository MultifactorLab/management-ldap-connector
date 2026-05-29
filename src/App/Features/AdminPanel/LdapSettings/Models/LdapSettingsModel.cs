namespace App.Features.AdminPanel.LdapSettings.Models;

public record LdapSettingsModel
{
    public required string ServerUrl { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public required string BaseDn { get; init; }
    public required string ServiceAccountDn { get; init; }

    /// <summary>
    /// Хранится зашифрованным через EncryptValueAdapter
    /// </summary>
    public required string BindPassword { get; init; }

    /// <summary>
    /// Атрибут для поиска пользователя: sAMAccountName / uid
    /// </summary>
    public required string UsernameAttribute { get; init; }

    public required string EmailAttribute { get; init; }
    public required string DisplayNameAttribute { get; init; }

    /// <summary>
    /// DN группы для синхронизации, например:
    /// CN=MGM Admins,OU=Groups,DC=company,DC=local
    /// Если null — синхронизируются все пользователи из BaseDn
    /// </summary>
    public string? SyncGroupDn { get; init; }
}