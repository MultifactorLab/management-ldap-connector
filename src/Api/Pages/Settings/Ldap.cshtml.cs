using System.ComponentModel.DataAnnotations;
using App.Features.AdminPanel.LdapSettings.GetLdapSettings;
using App.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using App.Features.AdminPanel.LdapSettings.SaveLdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages.Settings;

[Authorize]
public class LdapModel : AuthorizedPageModel
{
    private readonly IGetLdapSettingsUseCase _getLdapSettings;
    private readonly ISaveLdapSettingsUseCase _saveLdapSettings;
    private readonly ITestLdapConnectionUseCase _testLdapConnection;

    private const string DefaultUsernameAttribute = "sAMAccountName";
    private const string DefaultEmailAttribute = "mail";
    private const string DefaultDisplayNameAttribute = "displayName";

    public LdapModel(
        IGetLdapSettingsUseCase getLdapSettings,
        ISaveLdapSettingsUseCase saveLdapSettings,
        ITestLdapConnectionUseCase testLdapConnection)
    {
        _getLdapSettings = getLdapSettings;
        _saveLdapSettings = saveLdapSettings;
        _testLdapConnection = testLdapConnection;
    }

    [BindProperty] [Required] public string ServerUrl { get; set; }
    [BindProperty] [Range(1, 65535)] public int Port { get; set; } = 389;
    [BindProperty] public bool UseSsl { get; set; }
    [BindProperty] [Required] public string BaseDn { get; set; }
    [BindProperty] [Required] public string ServiceAccountDn { get; set; }
    [BindProperty] [Required] public string BindPassword { get; set; }
    [BindProperty] [Required] public string UsernameAttribute { get; set; } = DefaultUsernameAttribute;
    [BindProperty] [Required] public string EmailAttribute { get; set; } = DefaultEmailAttribute;
    [BindProperty] [Required] public string DisplayNameAttribute { get; set; } = DefaultDisplayNameAttribute;
    [BindProperty] public string? SyncGroupDn { get; set; }
    
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        var result = _getLdapSettings.Execute();

        if (result?.Settings is null)
        {
            return;
        }

        ServerUrl = result.Settings.ServerUrl;
        Port = result.Settings.Port;
        UseSsl = result.Settings.UseSsl;
        BaseDn = result.Settings.BaseDn;
        ServiceAccountDn = result.Settings.ServiceAccountDn;
        BindPassword = result.Settings.BindPassword;
        UsernameAttribute = result.Settings.UsernameAttribute;
        EmailAttribute = result.Settings.EmailAttribute;
        DisplayNameAttribute = result.Settings.DisplayNameAttribute;
        SyncGroupDn = result.Settings.SyncGroupDn;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _saveLdapSettings.Execute(new SaveLdapSettingsRequest(BuildSaveSettingsModel()));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error switch
            {
                SaveLdapSettingsError.InvalidSyncGroupDn =>
                    "Неверный формат DN группы синхронизации.",
                _ => "Ошибка при сохранении настроек."
            };
            return Page();
        }

        SuccessMessage = "Настройки сохранены.";
        return Page();
    }

    public IActionResult OnPostTest()
    {
        var result = _testLdapConnection.Execute(BuildTestConnectionModel());

        return new JsonResult(new
        {
            success = result.IsSuccess,
            message = result.IsSuccess
                ? "Подключение успешно."
                : result.Error switch
                {
                    TestLdapConnectionError.InvalidCredentials =>
                        "Неверные учётные данные Bind DN.",
                    TestLdapConnectionError.ConnectionFailed =>
                        $"Не удалось подключиться: {result.ErrorDetails}",
                    TestLdapConnectionError.SettingsNotConfigured =>
                        "Настройки не заданы.",
                    _ => "Неизвестная ошибка."
                }
        });
    }

    private SaveLdapSettingsModel BuildSaveSettingsModel() => new()
    {
        ServerUrl = ServerUrl,
        Port = Port,
        UseSsl = UseSsl,
        BaseDn = BaseDn,
        ServiceAccountDn = ServiceAccountDn,
        BindPassword = BindPassword,
        UsernameAttribute = UsernameAttribute,
        EmailAttribute = EmailAttribute,
        DisplayNameAttribute = DisplayNameAttribute,
        SyncGroupDn = SyncGroupDn
    };

    private TestLdapConnectionModel BuildTestConnectionModel() => new()
    {
        ServerUrl = ServerUrl,
        Port = Port,
        UseSsl = UseSsl,
        ServiceAccountDn = ServiceAccountDn,
        BindPassword = BindPassword,
    };
}