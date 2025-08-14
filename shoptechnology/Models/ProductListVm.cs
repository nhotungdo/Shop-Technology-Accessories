using System.Collections.Generic;

namespace ShopTechnologyAccessories.Models;

public class ProductListVm
{
    public IEnumerable<Product> Products { get; set; } = new List<Product>();

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize));

    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}


