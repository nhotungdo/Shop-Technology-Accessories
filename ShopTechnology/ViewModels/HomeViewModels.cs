namespace ShopTechnology.ViewModels
{
    public class HomeViewModel
    {
        public List<FeaturedProductViewModel> FeaturedProducts { get; set; } = new();
        public List<NewProductViewModel> NewProducts { get; set; } = new();
        public List<HotProductViewModel> HotProducts { get; set; } = new();
        public List<BannerViewModel> Banners { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
    }

    public class FeaturedProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsHot { get; set; }
        public bool IsOnSale { get; set; }
    }

    public class NewProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsHot { get; set; }
        public bool IsOnSale { get; set; }
    }

    public class HotProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsHot { get; set; }
        public bool IsOnSale { get; set; }
    }

    public class BannerViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public List<CategoryViewModel> Children { get; set; } = new();
    }
}
