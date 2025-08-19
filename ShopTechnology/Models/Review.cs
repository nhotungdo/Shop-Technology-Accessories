using System;
using System.Collections.Generic;

namespace ShopTechnology.Models;

public partial class Review
{
    public int ReviewId { get; set; }
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false; // Verified purchase
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
