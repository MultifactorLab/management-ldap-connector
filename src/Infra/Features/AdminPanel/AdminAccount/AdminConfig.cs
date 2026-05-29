using System.ComponentModel.DataAnnotations;

namespace Infra.Features.AdminPanel.AdminAccount;

public sealed class AdminConfig
{
    public const string SectionName = "Admin";
    
    [Required]
    public required string DefaultPassword { get; init; }
}