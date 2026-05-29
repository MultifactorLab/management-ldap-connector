using Api.Pages.Settings;
using App.SharedPorts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages;

public abstract class AuthorizedPageModel : PageModel
{
    [FromServices]
    public IGetAdminAccount GetAdminAccount { get; set; } = null!;
    
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var account = GetAdminAccount.Execute();

        if (account.MustChangePassword && context.HandlerInstance is not AccountModel)
        {
            context.Result = RedirectToPage("/Settings/Account",
                new { mustChange = true });
            return;
        }

        base.OnPageHandlerExecuting(context);
    }
}