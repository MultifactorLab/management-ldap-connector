namespace App.Features.AdminPanel.SyncStatus.GetSyncStatus.Models;

public record GetSyncStatusModel
{
    /// <summary>
    /// Время последнего запуска синхронизации
    /// </summary>
    public DateTime? LastRunAt { get; init; }

    /// <summary>
    /// Количество пользователей отправленных в MGM API
    /// </summary>
    public int SyncedCount { get; init; }

    public bool Success { get; init; }

    /// <summary>
    /// Текст ошибки если Success = false
    /// </summary>
    public string? ErrorMessage { get; init; }
}