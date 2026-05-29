using App.Features.AdminPanel.SyncStatus.GetSyncStatus;
using App.Features.BackgroundWorker.SyncAdminsToMgm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages.Sync;

[Authorize]
public class StatusModel : AuthorizedPageModel
{
    private readonly IGetSyncStatus _getSyncStatus;
    private readonly ISyncUsersUseCase _syncUsersUseCase;

    public StatusModel(
        IGetSyncStatus getSyncStatus,
        ISyncUsersUseCase syncUsersUseCase)
    {
        _getSyncStatus = getSyncStatus;
        _syncUsersUseCase = syncUsersUseCase;
    }

    public SyncStatus? Status { get; private set; }

    public void OnGet()
    {
        var result = _getSyncStatus.Execute();
        if (result is null)
        {
            return;
        }
        
        Status = new SyncStatus(
            LastRunAt: result.LastRunAt,
            SyncedCount: result.SyncedCount,
            Success: result.Success,
            ErrorMessage: result.ErrorMessage);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _syncUsersUseCase.Execute();

        Status = new SyncStatus(
            LastRunAt: DateTime.Now,
            SyncedCount: result.SyncedCount,
            Success: result.IsSuccess,
            ErrorMessage: result.ErrorMessage);

        return Page();
    }
}

public record SyncStatus(DateTime? LastRunAt, int SyncedCount, bool Success, string? ErrorMessage);