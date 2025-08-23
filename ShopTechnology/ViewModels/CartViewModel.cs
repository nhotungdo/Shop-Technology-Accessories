using System.ComponentModel.DataAnnotations;
using ShopTechnology.Models;

namespace ShopTechnology.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public string? AppliedPromotionCode { get; set; }
        public Promotion? AppliedPromotion { get; set; }
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;
        public int StockQuantity { get; set; } = 0;
    }

    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new CartViewModel();

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Thành phố")]
        public string? ShippingCity { get; set; }

        [Display(Name = "Tỉnh/Thành")]
        public string? ShippingProvince { get; set; }

        [Display(Name = "Mã bưu điện")]
        public string? ShippingPostalCode { get; set; }

        [Display(Name = "Ghi chú đơn hàng")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phương thức vận chuyển là bắt buộc")]
        [Display(Name = "Phương thức vận chuyển")]
        public string ShippingMethod { get; set; } = string.Empty;
    }

    public class PaymentViewModel
    {
        public Order Order { get; set; } = new Order();
        public List<string> PaymentMethods { get; set; } = new List<string>
        {
            "CreditCard",
            "BankTransfer",
            "Momo",
            "ZaloPay",
            "PayPal"
        };
    }
}
