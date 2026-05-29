namespace App.SharedPorts.Encryption.EncryptValue;

public interface IEncryptValue
{
    EncryptValueResult Execute(EncryptValueRequest request);
}

public sealed record EncryptValueRequest(string PlainText);

public sealed record EncryptValueResult(string CipherText);