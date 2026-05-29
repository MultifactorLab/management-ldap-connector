namespace Infra.SharedAdapters.LdapExecutor;

internal static class LdapErrorCodes
{
    /// <summary>
    /// Invalid credentials — неверные учётные данные при Bind.
    /// </summary>
    public const int InvalidCredentials = 49;
 
    /// <summary>
    /// Time limit exceeded — превышено время ожидания операции.
    /// </summary>
    public const int TimeLimitExceeded = 3;
 
    /// <summary>
    /// Busy — сервер занят.
    /// </summary>
    public const int Busy = 51;
 
    /// <summary>
    /// Unavailable — сервер недоступен.
    /// </summary>
    public const int Unavailable = 52;
 
    /// <summary>
    /// Local error / timeout — ошибка на стороне клиента или таймаут.
    /// </summary>
    public const int LocalError = 82;
 
    /// <summary>
    /// Timed out — истекло время ожидания ответа от сервера.
    /// </summary>
    public const int TimedOut = 85;
}