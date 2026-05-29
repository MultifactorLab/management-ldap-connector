using System.Security.Cryptography;
using App.SharedPorts.Encryption.EncryptValue;
using Infra.SharedAdapters.Encryption;
using Infra.SharedAdapters.Encryption.EncryptValue;
using Microsoft.Extensions.Options;

namespace Infra.Tests.SharedAdapters;

public class EncryptValueAdapterTests
{
    private readonly EncryptValueAdapter _adapter;
 
    public EncryptValueAdapterTests()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var options = Options.Create(new EncryptionConfig { Key = key });
 
        _adapter = new EncryptValueAdapter(options);
    }
 
    [Fact]
    public void Execute_WhenPlainTextIsProvided_ReturnCipherText()
    {
        var result = _adapter.Execute(new EncryptValueRequest("secret"));
 
        Assert.False(string.IsNullOrEmpty(result.CipherText));
        Assert.NotEqual("secret", result.CipherText);
    }
 
    [Fact]
    public void Execute_SamePlainText_ProducesDifferentCipherTextEachTime()
    {
        var result1 = _adapter.Execute(new EncryptValueRequest("secret"));
        var result2 = _adapter.Execute(new EncryptValueRequest("secret"));
 
        // разный IV каждый раз — шифротексты должны отличаться
        Assert.NotEqual(result1.CipherText, result2.CipherText);
    }
 
    [Fact]
    public void Constructor_WhenKeyIsWrongLength_ThrowsInvalidOperationException()
    {
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)); // 128 бит вместо 256
        var options = Options.Create(new EncryptionConfig { Key = shortKey });

 
        Assert.Throws<InvalidOperationException>(() => new EncryptValueAdapter(options));
    }
}