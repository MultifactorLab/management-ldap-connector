using System.Security.Cryptography;
using System.Text;
using App.SharedPorts.Encryption.DecryptValue;
using Microsoft.Extensions.Options;

namespace Infra.SharedAdapters.Encryption.DecryptValue;

/// <summary>
/// Дешифрует строку зашифрованную через EncryptValueAdapter.
/// Ожидает Base64(IV[16 байт] + CipherText).
/// </summary>
internal sealed class DecryptValueAdapter : IDecryptValue
{
    private readonly byte[] _key;

    public DecryptValueAdapter(IOptions<EncryptionConfig> options)
    {
        var keyBase64 = options.Value.Key;

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("Encryption:Key must be 256 bits (32 bytes) base64-encoded.");
        }
    }

    public DecryptValueResult Execute(DecryptValueRequest request)
    {
        if (string.IsNullOrEmpty(request.CipherText))
            return new DecryptValueResult(string.Empty);

        var fullBytes = Convert.FromBase64String(request.CipherText);

        if (fullBytes.Length < 16)
        {
            throw new CryptographicException("Invalid cipher text: too short to contain IV.");
        }

        // извлекаем IV из первых 16 байт
        var iv = fullBytes[..16];
        var cipherBytes = fullBytes[16..];

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, inputOffset: 0, cipherBytes.Length);

        return new DecryptValueResult(Encoding.UTF8.GetString(plainBytes));
    }
}