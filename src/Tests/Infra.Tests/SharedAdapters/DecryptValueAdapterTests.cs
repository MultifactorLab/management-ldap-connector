using System.Security.Cryptography;
using App.SharedPorts.Encryption.DecryptValue;
using App.SharedPorts.Encryption.EncryptValue;
using Infra.SharedAdapters.Encryption;
using Infra.SharedAdapters.Encryption.DecryptValue;
using Infra.SharedAdapters.Encryption.EncryptValue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infra.Tests.SharedAdapters;

public class DecryptValueAdapterTests
{
    private readonly EncryptValueAdapter _encryptAdapter;
    private readonly DecryptValueAdapter _decryptAdapter;
 
    public DecryptValueAdapterTests()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var options = Options.Create(new EncryptionConfig { Key = key });
 
        _encryptAdapter = new EncryptValueAdapter(options);
        _decryptAdapter = new DecryptValueAdapter(options);
    }
 
    [Fact]
    public void Execute_WhenCipherTextIsValid_ReturnOriginalPlainText()
    {
        const string original = "secret";
        var encrypted = _encryptAdapter.Execute(new EncryptValueRequest(original));
 
        var result = _decryptAdapter.Execute(new DecryptValueRequest(encrypted.CipherText));
 
        Assert.Equal(original, result.PlainText);
    }
 
    [Fact]
    public void Execute_WhenCipherTextIsCorrupted_ThrowsException()
    {
        var encrypted = _encryptAdapter.Execute(new EncryptValueRequest("secret"));
        var corrupted = encrypted.CipherText[..^4] + "XXXX"; // изменяем строку
 
        Assert.ThrowsAny<Exception>(() => _decryptAdapter.Execute(new DecryptValueRequest(corrupted)));
    }
}