using System.ComponentModel.DataAnnotations;

namespace Infra.Integrations.LiteDb;

public sealed class LiteDbConfig
{
    public const string SectionName = "LiteDb";

    [Required]
    public required string Path { get; init; }

    public string ConnectionString => $"Filename={Path};Upgrade=true";
}