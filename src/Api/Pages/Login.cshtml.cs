using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using App.Features.AdminPanel.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages;

public class LoginModel : PageModel
{
    private readonly ILoginUseCase _loginUseCase;

    public LoginModel(ILoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [BindProperty] [Required] public string Password { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Settings/Ldap");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _loginUseCase.Execute(new LoginRequest(Password));

        if (!result.IsSuccess)
        {
            ErrorMessage = "Неверный пароль.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "admin")
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        if (result.MustChangePassword)
        {
            return RedirectToPage("/Settings/Account", new { mustChange = true });
        }

        return RedirectToPage("/Settings/Ldap");
    }
}