using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;

namespace App.Features.Api.Authenticate;

public interface IAuthenticateLdapUser
{
    AuthenticateLdapUserResult Execute(LdapSettingsModel settings, UserCredential credential);
}

public record UserCredential(string Username, string Password);

public enum AuthenticateLdapUserError
{
    UserNotFound,
    InvalidCredentials,
    ConnectionFailed
}

public sealed record AuthenticateLdapUserResult
{
    public bool IsSuccess { get; init; }
    public string DisplayName { get; init; } = null!;
    public AuthenticateLdapUserError? Error { get; init; }

    public static AuthenticateLdapUserResult Success(string displayName) => new()
    {
        IsSuccess = true,
        DisplayName = displayName
    };

    public static AuthenticateLdapUserResult Failure(AuthenticateLdapUserError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}