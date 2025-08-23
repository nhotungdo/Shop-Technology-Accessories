using System.Collections.Generic;

namespace ShopTechnology.Models
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public List<Product> LatestProducts { get; set; } = new List<Product>();
        public List<Product> NewProducts { get; set; } = new List<Product>();
        public List<Product> HotProducts { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Banner> Banners { get; set; } = new List<Banner>();
    }
}
