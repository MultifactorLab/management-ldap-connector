using App.Features.AdminPanel.LdapSettings.Models;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;
using Microsoft.Extensions.Logging;

namespace App.Features.Api.Authenticate;

public interface IAuthenticateUseCase
{
    AuthenticateResult Execute(AuthenticateRequest request);
}

internal sealed class AuthenticateUseCase : IAuthenticateUseCase
{
    private readonly IGetLdapSettings _getLdapSettings;
    private readonly IDecryptValue _decryptValue;
    private readonly IAuthenticateLdapUser _authenticateLdapUser;
    private readonly ILogger<AuthenticateUseCase> _logger;

    public AuthenticateUseCase(IGetLdapSettings getLdapSettings,
        IDecryptValue decryptValue,
        IAuthenticateLdapUser authenticateLdapUser,
        ILogger<AuthenticateUseCase> logger)
    {
        _getLdapSettings = getLdapSettings;
        _decryptValue = decryptValue;
        _authenticateLdapUser = authenticateLdapUser;
        _logger = logger;
    }

    public AuthenticateResult Execute(AuthenticateRequest request)
    {
        var settings = _getLdapSettings.Execute();

        if (settings is null)
        {
            _logger.LogWarning("Authentication failed for {Username:l}: LDAP settings not configured.",
                request.Username);
            return AuthenticateResult.Failure(AuthenticateError.SettingsNotConfigured);
        }

        var decryptResult = _decryptValue.Execute(
            new DecryptValueRequest(settings.BindPassword));

        var ldapSettings = new LdapSettingsModel
        {
            ServerUrl = settings.ServerUrl,
            Port = settings.Port,
            UseSsl = settings.UseSsl,
            BaseDn = settings.BaseDn,
            ServiceAccountDn = settings.ServiceAccountDn,
            BindPassword = decryptResult.PlainText,
            UsernameAttribute = settings.UsernameAttribute,
            EmailAttribute = settings.EmailAttribute,
            DisplayNameAttribute = settings.DisplayNameAttribute,
            SyncGroupDn = settings.SyncGroupDn
        };

        var result = _authenticateLdapUser.Execute(
            ldapSettings,
            new UserCredential(request.Username, request.Password));

        if (result.IsSuccess)
        {
            _logger.LogInformation("User {Username:l} authenticated successfully.", request.Username);
            return AuthenticateResult.Success(result.DisplayName);
        }

        var error = result.Error switch
        {
            AuthenticateLdapUserError.UserNotFound => AuthenticateError.UserNotFound,
            AuthenticateLdapUserError.InvalidCredentials => AuthenticateError.InvalidCredentials,
            AuthenticateLdapUserError.ConnectionFailed => AuthenticateError.LdapUnavailable,
            _ => AuthenticateError.LdapUnavailable
        };

        _logger.LogWarning("Authentication failed for {Username}: {Error}.",
            request.Username, error);

        return AuthenticateResult.Failure(error);
    }
}

public sealed record AuthenticateRequest(string Username, string Password);

public enum AuthenticateError
{
    SettingsNotConfigured,
    UserNotFound,
    InvalidCredentials,
    LdapUnavailable
}

public sealed record AuthenticateResult
{
    public bool IsSuccess { get; init; }
    public string? DisplayName { get; init; }
    public AuthenticateError? Error { get; init; }

    public static AuthenticateResult Success(string displayName) => new()
    {
        IsSuccess = true,
        DisplayName = displayName
    };

    public static AuthenticateResult Failure(AuthenticateError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}