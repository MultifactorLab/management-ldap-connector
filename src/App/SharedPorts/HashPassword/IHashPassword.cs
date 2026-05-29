namespace App.SharedPorts.HashPassword;

public interface IHashPassword
{
    HashPasswordResult Execute(HashPasswordRequest request);
}

public sealed record HashPasswordRequest(string Password);

public sealed record HashPasswordResult(string Hash);