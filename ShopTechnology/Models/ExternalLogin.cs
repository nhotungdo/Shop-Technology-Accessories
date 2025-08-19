using System;

namespace ShopTechnology.Models;

public partial class ExternalLogin
{
    public int ExternalLoginId { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, Facebook
    public string ProviderKey { get; set; } = string.Empty; // ID từ provider
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual User User { get; set; } = null!;
}
