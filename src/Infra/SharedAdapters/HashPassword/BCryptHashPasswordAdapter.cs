using App.SharedPorts.HashPassword;

namespace Infra.SharedAdapters.HashPassword;

internal sealed class BCryptHashPasswordAdapter : IHashPassword
{
    public HashPasswordResult Execute(HashPasswordRequest request)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        return new HashPasswordResult(hash);
    }
}