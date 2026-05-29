namespace App.SharedPorts.VerifyPassword;

public interface IVerifyPassword
{
    VerifyPasswordResult Execute(VerifyPasswordRequest request);
}

public sealed record VerifyPasswordRequest(string Password, string Hash);
 
public sealed record VerifyPasswordResult(bool IsValid);