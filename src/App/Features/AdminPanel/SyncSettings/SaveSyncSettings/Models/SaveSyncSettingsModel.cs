namespace App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;

public record SaveSyncSettingsModel
{
    public int IntervalMinutes { get; init; } = 60;
    public required string ApiUrl { get; init; }
    public required string ApiKey { get; init; }

    /// <summary>
    /// Хранится зашифрованным через EncryptValueAdapter
    /// </summary>
    public required string ApiSecret { get; init; }
}