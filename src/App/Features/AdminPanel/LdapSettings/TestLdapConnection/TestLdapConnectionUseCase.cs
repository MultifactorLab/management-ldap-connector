using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Microsoft.Extensions.Logging;

namespace App.Features.AdminPanel.LdapSettings.TestLdapConnection;

public interface ITestLdapConnectionUseCase
{
    TestLdapConnectionResult Execute(TestLdapConnectionModel settingsModel);
}

internal sealed class TestLdapConnectionUseCase : ITestLdapConnectionUseCase
{
    private readonly ITestLdapConnection _testLdapConnection;
    private readonly ILogger<TestLdapConnectionUseCase> _logger;

    public TestLdapConnectionUseCase(ITestLdapConnection testLdapConnection,
        ILogger<TestLdapConnectionUseCase> logger)
    {
        _testLdapConnection = testLdapConnection;
        _logger = logger;
    }

    public TestLdapConnectionResult Execute(TestLdapConnectionModel testLdapConnectionModel)
    {
        var result = _testLdapConnection.Execute(testLdapConnectionModel);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Test LDAP connection failed: {Error} — {Details:l}.",
                result.Error, result.ErrorDetails);
        }
        else
        {
            _logger.LogInformation("Test LDAP connection succeeded.");
        }

        return result;
    }
}

public enum TestLdapConnectionError
{
    SettingsNotConfigured,
    ConnectionFailed,
    InvalidCredentials
}

public sealed record TestLdapConnectionResult
{
    public bool IsSuccess { get; init; }
    public TestLdapConnectionError? Error { get; init; }
    public string? ErrorDetails { get; init; }

    public static TestLdapConnectionResult Success() => new() { IsSuccess = true };

    public static TestLdapConnectionResult Failure(
        TestLdapConnectionError error,
        string? details = null) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorDetails = details
    };
}