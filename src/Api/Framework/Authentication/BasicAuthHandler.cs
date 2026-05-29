using System.Security.Claims;
using System.Text.Encodings.Web;
using App.Features.AdminPanel.SyncSettings.GetSyncSettings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Framework.Authentication;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IGetSyncSettingsUseCase _getSyncSettings;

    public BasicAuthHandler(
        IGetSyncSettingsUseCase getSyncSettings,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        _getSyncSettings = getSyncSettings;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var credential = BasicAuthCredential.GetFromHeaders(Request.Headers);

        if (credential is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var getSettings = _getSyncSettings.Execute();

        if (getSettings?.Settings is null)
        {
            Logger.LogWarning("Basic auth failed: sync settings not configured.");
            return Task.FromResult(AuthenticateResult.Fail("Sync settings not configured"));
        }

        if (credential.Username != getSettings.Settings.ApiKey ||
            credential.Password != getSettings.Settings.ApiSecret)
        {
            Logger.LogInformation("Basic auth failed: invalid ApiKey or ApiSecret.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid ApiKey or ApiSecret"));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, credential.Username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}