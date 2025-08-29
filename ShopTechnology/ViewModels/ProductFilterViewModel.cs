namespace ShopTechnology.ViewModels
{
    public class ProductFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsHot { get; set; }
        public string? SortBy { get; set; }
    }
}
