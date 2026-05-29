using System.Security.Cryptography;
using System.Text;
using App.SharedPorts.Encryption.EncryptValue;
using Microsoft.Extensions.Options;

namespace Infra.SharedAdapters.Encryption.EncryptValue;

/// <summary>
/// Шифрует строку через AES-256-CBC.
/// IV генерируется случайно при каждом вызове и сохраняется
/// вместе с шифротекстом: Base64(IV[16 байт] + CipherText).
/// </summary>
internal sealed class EncryptValueAdapter : IEncryptValue
{
    private readonly byte[] _key;

    public EncryptValueAdapter(IOptions<EncryptionConfig> options)
    {
        var keyBase64 = options.Value.Key;

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("Encryption:Key must be 256 bits (32 bytes) base64-encoded.");
        }
    }

    public EncryptValueResult Execute(EncryptValueRequest request)
    {
        if (string.IsNullOrEmpty(request.PlainText))
        {
            return new EncryptValueResult(string.Empty);
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // случайный IV — одинаковый вход даёт разный шифротекст

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(request.PlainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, inputOffset: 0, plainBytes.Length);

        // IV(16 байт) + CipherText
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);

        return new EncryptValueResult(Convert.ToBase64String(result));
    }
}