using System.ComponentModel.DataAnnotations;
using App.Features.AdminPanel.AdminAccount.ChangeAdminPassword;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages.Settings;

[Authorize]
public class AccountModel : PageModel
{
    private readonly IChangeAdminPasswordUseCase _changeAdminPasswordUseCase;

    public AccountModel(IChangeAdminPasswordUseCase changeAdminPasswordUseCase)
    {
        _changeAdminPasswordUseCase = changeAdminPasswordUseCase;
    }

    [BindProperty(SupportsGet = true)] public bool MustChange { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Введите новый пароль")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
    public string NewPassword { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Подтвердите пароль")]
    public string ConfirmPassword { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet() => Page();

    public IActionResult OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return Page();
        }

        _changeAdminPasswordUseCase.Execute(new ChangeAdminPasswordModel(NewPassword));

        if (MustChange)
        {
            return RedirectToPage("/Settings/Ldap");
        }

        SuccessMessage = "Пароль успешно изменён.";
        return Page();
    }
}