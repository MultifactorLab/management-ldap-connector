using System.ComponentModel.DataAnnotations;

namespace Infra.SharedAdapters.Encryption;

public sealed class EncryptionConfig
{
    public const string SectionName = "Encryption";
    
    [Required]
    public required string Key { get; init; }
}