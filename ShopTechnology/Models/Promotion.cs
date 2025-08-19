using System;
using System.Collections.Generic;

namespace ShopTechnology.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
