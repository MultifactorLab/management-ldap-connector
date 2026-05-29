using App.SharedPorts.VerifyPassword;

namespace Infra.SharedAdapters.VerifyPassword;

internal sealed class BCryptVerifyPasswordAdapter : IVerifyPassword
{
    public VerifyPasswordResult Execute(VerifyPasswordRequest request)
    {
        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, request.Hash);
        return new VerifyPasswordResult(isValid);
    }
}