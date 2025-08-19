using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class OrderDTO
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
    
    public Guid? PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string ShippingAddress { get; set; } = string.Empty;
    
    public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    
    // Computed properties
    public string StatusDisplay => Status switch
    {
        "Pending" => "Chờ xử lý",
        "Paid" => "Đã thanh toán",
        "Shipped" => "Đang giao hàng",
        "Completed" => "Hoàn thành",
        "Canceled" => "Đã hủy",
        _ => Status
    };
    
    public bool CanCancel => Status == "Pending" || Status == "Paid";
    public bool CanUpdateStatus => Status != "Completed" && Status != "Canceled";
}

public class CreateOrderDTO
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [StringLength(255)]
    public string ShippingAddress { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;
    
    public List<CreateOrderDetailDTO> OrderDetails { get; set; } = new();
}

public class UpdateOrderStatusDTO
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

public class OrderDetailDTO
{
    public int OrderDetailId { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    public decimal SubTotal => Quantity * Price;
}

public class CreateOrderDetailDTO
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
