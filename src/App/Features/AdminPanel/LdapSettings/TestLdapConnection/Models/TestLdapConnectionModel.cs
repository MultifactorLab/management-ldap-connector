namespace App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;

public class TestLdapConnectionModel
{
    public required string ServerUrl { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public required string ServiceAccountDn { get; init; }

    /// <summary>
    /// Хранится зашифрованным через EncryptValueAdapter
    /// </summary>
    public required string BindPassword { get; init; }
}