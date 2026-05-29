using System.ComponentModel.DataAnnotations;
using App.Features.AdminPanel.SyncSettings.GetSyncSettings;
using App.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using App.Features.AdminPanel.SyncSettings.SaveSyncSettings.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages.Settings;

[Authorize]
public class SyncModel : AuthorizedPageModel
{
    private readonly IGetSyncSettingsUseCase _getSyncSettings;
    private readonly ISaveSyncSettingsUseCase _saveSyncSettings;

    public SyncModel(
        IGetSyncSettingsUseCase getSyncSettings,
        ISaveSyncSettingsUseCase saveSyncSettings)
    {
        _getSyncSettings = getSyncSettings;
        _saveSyncSettings = saveSyncSettings;
    }

    [BindProperty]
    [Range(1, int.MaxValue, ErrorMessage = "Интервал должен быть больше 0")]
    public int IntervalMinutes { get; set; } = 60;

    [BindProperty] [Required] public string ApiUrl { get; set; }
    [BindProperty] [Required] public string ApiKey { get; set; }
    [BindProperty] public string ApiSecret { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        var result = _getSyncSettings.Execute();

        if (result is null)
        {
            return;
        }

        var s = result;
        IntervalMinutes = result.Settings.IntervalMinutes;
        ApiUrl = result.Settings.ApiUrl;
        ApiKey = result.Settings.ApiKey;
        ApiSecret = result.Settings.ApiSecret;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _saveSyncSettings.Execute(new SaveSyncSettingsRequest(
            new SaveSyncSettingsModel()
            {
                IntervalMinutes = IntervalMinutes,
                ApiUrl = ApiUrl,
                ApiKey = ApiKey,
                ApiSecret = ApiSecret
            }));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error switch
            {
                SaveSyncSettingsError.InvalidIntervalMinutes =>
                    "Интервал синхронизации должен быть больше 0.",
                _ => "Ошибка при сохранении настроек."
            };
            return Page();
        }

        SuccessMessage = "Настройки сохранены.";
        return Page();
    }
}