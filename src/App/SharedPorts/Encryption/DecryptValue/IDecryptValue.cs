namespace App.SharedPorts.Encryption.DecryptValue;

public interface IDecryptValue
{
    DecryptValueResult Execute(DecryptValueRequest request);
}

public sealed record DecryptValueRequest(string CipherText);

public sealed record DecryptValueResult(string PlainText);