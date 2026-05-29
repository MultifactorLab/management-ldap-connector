using App.SharedPorts;
using App.SharedPorts.VerifyPassword;
using Microsoft.Extensions.Logging;

namespace App.Features.AdminPanel.Login;

public interface ILoginUseCase
{
    LoginResult Execute(LoginRequest request);
}

internal sealed class LoginUseCase : ILoginUseCase
{
    private readonly IGetAdminAccount _getAdminAccount;
    private readonly IVerifyPassword _verifyPassword;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        IGetAdminAccount getAdminAccount,
        IVerifyPassword verifyPassword,
        ILogger<LoginUseCase> logger)
    {
        _getAdminAccount = getAdminAccount;
        _verifyPassword = verifyPassword;
        _logger = logger;
    }

    public LoginResult Execute(LoginRequest request)
    {
        var account = _getAdminAccount.Execute();

        var verifyResult = _verifyPassword.Execute(
            new VerifyPasswordRequest(request.Password, account.PasswordHash));

        if (!verifyResult.IsValid)
        {
            _logger.LogWarning("Login attempt failed: invalid credentials for user {Username:l}.",
                account.Username);
            return LoginResult.Failure(LoginError.InvalidCredentials);
        }

        _logger.LogInformation("User {Username:l} logged in successfully.", account.Username);

        return LoginResult.Success(account.MustChangePassword);
    }
}

public sealed record LoginRequest(string Password);

public enum LoginError
{
    InvalidCredentials
}

public sealed record LoginResult
{
    public bool IsSuccess { get; init; }
    public bool MustChangePassword { get; init; }
    public LoginError? Error { get; init; }

    public static LoginResult Success(bool mustChangePassword) => new()
    {
        IsSuccess = true,
        MustChangePassword = mustChangePassword
    };

    public static LoginResult Failure(LoginError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}