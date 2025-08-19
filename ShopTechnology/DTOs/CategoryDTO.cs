using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class CategoryDTO
{
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int ProductCount { get; set; }
}

public class CreateCategoryDTO
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}

public class UpdateCategoryDTO
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}
