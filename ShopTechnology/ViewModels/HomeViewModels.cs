using System.ComponentModel.DataAnnotations;
using ShopTechnology.Models;

namespace ShopTechnology.ViewModels;

public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public string SearchTerm { get; set; } = string.Empty;
    public List<ProductViewModel> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<string> Brands { get; set; } = new();
}

public class CategoryViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public List<ProductViewModel> Products { get; set; } = new();
    public List<CategoryViewModel> SubCategories { get; set; } = new();
    public Category Category { get; set; } = new();
    public string? SortBy { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}

public class ProductDetailViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsInWishlist { get; set; }
    public bool IsInCart { get; set; }
    public int CartQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductReviewViewModel> Reviews { get; set; } = new();
    public List<ProductViewModel> RelatedProducts { get; set; } = new();
    public Product Product { get; set; } = new();
}

public class ProductReviewViewModel
{
    public int ReviewId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
