using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string ProductName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; }
        
        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }
        
        public string CategoryName { get; set; } = string.Empty;
        
        public List<string> ImageUrls { get; set; } = new List<string>();
        
        public string MainImageUrl { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
