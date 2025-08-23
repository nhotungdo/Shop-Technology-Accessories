using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class CartService : ICartService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public CartService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<CartViewModel> GetCartAsync(int? userId)
        {
            var cartViewModel = new CartViewModel();

            if (!userId.HasValue)
            {
                return cartViewModel;
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null)
            {
                // Create new cart if doesn't exist
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Convert cart items to view model
            foreach (var item in cart.CartItems)
            {
                var cartItemViewModel = new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSKU = item.Product.SKU ?? "",
                    ProductImage = item.Product.MainImage ?? "",
                    ProductBrand = item.Product.Brand ?? "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    StockQuantity = item.Product.StockQuantity
                };
                cartViewModel.Items.Add(cartItemViewModel);
            }

            // Calculate totals
            cartViewModel.SubTotal = cartViewModel.Items.Sum(i => i.TotalPrice);
            cartViewModel.TaxAmount = cartViewModel.SubTotal * 0.1m; // 10% tax
            cartViewModel.ShippingFee = cartViewModel.SubTotal > 500000 ? 0 : 30000; // Free shipping over 500k
            cartViewModel.TotalAmount = cartViewModel.SubTotal + cartViewModel.TaxAmount + cartViewModel.ShippingFee - cartViewModel.DiscountAmount;

            return cartViewModel;
        }

        public async Task<ServiceResult> AddToCartAsync(int? userId, int productId, int quantity)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." };
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại." };
            }

            if (!product.IsActive)
            {
                return new ServiceResult { Success = false, Message = "Sản phẩm hiện không khả dụng." };
            }

            if (product.StockQuantity < quantity)
            {
                return new ServiceResult { Success = false, Message = "Số lượng sản phẩm trong kho không đủ." };
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                existingItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * quantity,
                    CreatedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Sản phẩm đã được thêm vào giỏ hàng." };
        }

        public async Task<ServiceResult> UpdateQuantityAsync(int? userId, int cartItemId, int quantity)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập." };
            }

            if (quantity <= 0)
            {
                return new ServiceResult { Success = false, Message = "Số lượng phải lớn hơn 0." };
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại trong giỏ hàng." };
            }

            if (cartItem.Product.StockQuantity < quantity)
            {
                return new ServiceResult { Success = false, Message = "Số lượng sản phẩm trong kho không đủ." };
            }

            cartItem.Quantity = quantity;
            cartItem.TotalPrice = quantity * cartItem.UnitPrice;
            cartItem.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Số lượng đã được cập nhật." };
        }

        public async Task<ServiceResult> RemoveFromCartAsync(int? userId, int cartItemId)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập." };
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại trong giỏ hàng." };
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Sản phẩm đã được xóa khỏi giỏ hàng." };
        }

        public async Task<ServiceResult> ClearCartAsync(int? userId)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập." };
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
            }

            return new ServiceResult { Success = true, Message = "Giỏ hàng đã được làm trống." };
        }

        public async Task<ServiceResult> ApplyPromotionAsync(int? userId, string promotionCode)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập." };
            }

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == promotionCode && p.IsActive && 
                                         p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);

            if (promotion == null)
            {
                return new ServiceResult { Success = false, Message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn." };
            }

            if (promotion.UsageLimit.HasValue && promotion.UsedCount >= promotion.UsageLimit.Value)
            {
                return new ServiceResult { Success = false, Message = "Mã khuyến mãi đã hết lượt sử dụng." };
            }

            // For now, we'll just return success. In a real implementation,
            // you would store the applied promotion in the cart or session
            return new ServiceResult { Success = true, Message = "Mã khuyến mãi đã được áp dụng." };
        }

        public async Task<ServiceResult> RemovePromotionAsync(int? userId)
        {
            if (!userId.HasValue)
            {
                return new ServiceResult { Success = false, Message = "Vui lòng đăng nhập." };
            }

            // For now, we'll just return success. In a real implementation,
            // you would remove the applied promotion from the cart or session
            return new ServiceResult { Success = true, Message = "Mã khuyến mãi đã được xóa." };
        }
    }
}
