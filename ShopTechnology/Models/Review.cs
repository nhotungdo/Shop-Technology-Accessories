using System;

namespace ShopTechnology.Models;

public partial class Review
{
    public int ReviewId { get; set; }
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
