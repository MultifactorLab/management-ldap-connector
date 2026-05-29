using App.Features.AdminPanel.SyncSettings.GetSyncSettings.Models;
using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;

namespace Infra.Features.AdminPanel.SyncSettings.Models;

internal sealed class DbSyncSettings
{
    public const string SyncSettingsId = "sync-settings";
    public string Id { get; private set; } = SyncSettingsId;
    public int IntervalMinutes { get; set; } = 60;
    public required string ApiUrl { get; set; }
    public required string ApiKey { get; set; }

    /// <summary>Хранится зашифрованным через EncryptValueAdapter</summary>
    public required string ApiSecret { get; set; }

    public static GetSyncSettingsModel ToAppModel(DbSyncSettings dbModel)
    {
        return new GetSyncSettingsModel
        {
            ApiKey = dbModel.ApiKey,
            ApiSecret = dbModel.ApiSecret,
            IntervalMinutes = dbModel.IntervalMinutes,
            ApiUrl = dbModel.ApiUrl,
        };
    }

    public static DbSyncSettings FromAppModel(SaveSyncSettingsModel model)
    {
        return new DbSyncSettings
        {
            ApiKey = model.ApiKey,
            ApiSecret = model.ApiSecret,
            IntervalMinutes = model.IntervalMinutes,
            ApiUrl = model.ApiUrl,
        };
    }
}