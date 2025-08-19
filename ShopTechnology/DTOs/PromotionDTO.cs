using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class PromotionDTO
{
    public int PromotionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
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

    // Computed properties
    public bool IsValid => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate && UsedCount < MaxUsageCount;
    public string Status => IsValid ? "Có hiệu lực" : "Hết hiệu lực";
    public int RemainingUses => MaxUsageCount - UsedCount;
}

public class CreatePromotionDTO
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxUsageCount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdatePromotionDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxUsageCount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }
}

public class ApplyPromotionDTO
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OrderAmount { get; set; }
}
