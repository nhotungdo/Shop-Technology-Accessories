using System.Collections.Generic;

namespace ShopTechnology.Models
{
    public class HomeViewModel
    {
        // Hero Banner - Sản phẩm nổi bật
        public List<Product> HeroProducts { get; set; } = new List<Product>();

        // Categories - Danh mục sản phẩm
        public List<Category> Categories { get; set; } = new List<Category>();

        // Best Sellers - Sản phẩm bán chạy
        public List<Product> BestSellers { get; set; } = new List<Product>();

        // New Arrivals - Sản phẩm mới về
        public List<Product> NewArrivals { get; set; } = new List<Product>();

        // Promotions - Khuyến mãi và deal đặc biệt
        public List<Product> PromotionalProducts { get; set; } = new List<Product>();

        // Personalized Recommendations - Khuyến nghị cá nhân hóa
        public List<Product> RecommendedProducts { get; set; } = new List<Product>();

        // Banners
        public List<Banner> Banners { get; set; } = new List<Banner>();

        // Legacy properties for backward compatibility
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public List<Product> LatestProducts { get; set; } = new List<Product>();
        public List<Product> NewProducts { get; set; } = new List<Product>();
        public List<Product> HotProducts { get; set; } = new List<Product>();
    }
}
