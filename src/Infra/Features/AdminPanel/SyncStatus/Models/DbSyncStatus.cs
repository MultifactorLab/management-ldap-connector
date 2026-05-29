using App.Features.AdminPanel.SyncStatus.GetSyncStatus.Models;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;

namespace Infra.Features.AdminPanel.SyncStatus.Models;

internal sealed class DbSyncStatus
{
    public const string SyncStatusId = "sync-status";
    public string Id { get; private set; } = SyncStatusId;

    /// <summary>Время последнего запуска синхронизации</summary>
    public DateTime? LastRunAt { get; set; }

    /// <summary>Количество пользователей отправленных в MGM API</summary>
    public int SyncedCount { get; set; }

    public bool Success { get; set; }

    /// <summary>Текст ошибки если Success = false</summary>
    public string? ErrorMessage { get; set; }

    public static GetSyncStatusModel ToAppModel(DbSyncStatus dbModel)
    {
        return new GetSyncStatusModel
        {
            ErrorMessage = dbModel.ErrorMessage,
            LastRunAt = dbModel.LastRunAt,
            SyncedCount = dbModel.SyncedCount,
            Success = dbModel.Success
        };
    }

    public static DbSyncStatus FromAppModel(SaveSyncStatusModel model)
    {
        return new DbSyncStatus
        {
            ErrorMessage = model.ErrorMessage,
            LastRunAt = model.LastRunAt,
            SyncedCount = model.SyncedCount,
            Success = model.Success
        };
    }
}