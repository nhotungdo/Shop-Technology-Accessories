using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartAsync(string userId, string? sessionId = null)
        {
            var query = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images.Where(img => img.IsMain));

            if (!string.IsNullOrEmpty(userId))
            {
                return await query.FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await query.FirstOrDefaultAsync(c => c.SessionId == sessionId);
            }

            return null;
        }

        public async Task<Cart> CreateCartAsync(string userId, string? sessionId = null)
        {
            var cart = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity, string? sessionId = null)
        {
            try
            {
                var cart = await GetCartAsync(userId, sessionId);
                if (cart == null)
                {
                    cart = await CreateCartAsync(userId, sessionId);
                }

                var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null) return false;

                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        CreatedAt = DateTime.UtcNow
                    };

                    cart.Items.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null) return false;

                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null) return false;

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(int cartId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == cartId);

                if (cart == null) return false;

                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CartViewModel> GetCartViewModelAsync(string userId, string? sessionId = null)
        {
            var cart = await GetCartAsync(userId, sessionId);
            if (cart == null)
            {
                return new CartViewModel
                {
                    CartId = 0,
                    Items = new List<CartItemViewModel>(),
                    Subtotal = 0,
                    TaxAmount = 0,
                    ShippingAmount = 0,
                    TotalAmount = 0,
                    ItemCount = 0
                };
            }

            var items = cart.Items.Select(ci => new CartItemViewModel
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductImage = ci.Product.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl,
                UnitPrice = ci.UnitPrice,
                Quantity = ci.Quantity,
                TotalPrice = ci.UnitPrice * ci.Quantity
            }).ToList();

            var subtotal = items.Sum(item => item.TotalPrice);
            var taxAmount = subtotal * 0.1m; // 10% tax
            var shippingAmount = subtotal > 200 ? 0 : 10; // Free shipping over $200
            var totalAmount = subtotal + taxAmount + shippingAmount;

            return new CartViewModel
            {
                CartId = cart.Id,
                Items = items,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                TotalAmount = totalAmount,
                ItemCount = items.Count
            };
        }

        public async Task<bool> MergeGuestCartAsync(string userId, string sessionId)
        {
            try
            {
                var guestCart = await GetCartAsync(null, sessionId);
                var userCart = await GetCartAsync(userId);

                if (guestCart == null) return true;

                if (userCart == null)
                {
                    // Create new cart for user
                    userCart = await CreateCartAsync(userId);
                }

                foreach (var guestItem in guestCart.Items)
                {
                    var existingItem = userCart.Items.FirstOrDefault(ci => ci.ProductId == guestItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += guestItem.Quantity;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newItem = new CartItem
                        {
                            CartId = userCart.Id,
                            ProductId = guestItem.ProductId,
                            Quantity = guestItem.Quantity,
                            UnitPrice = guestItem.UnitPrice,
                            CreatedAt = DateTime.UtcNow
                        };
                        userCart.Items.Add(newItem);
                    }
                }

                // Remove guest cart
                _context.Carts.Remove(guestCart);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId, string? sessionId = null)
        {
            var cart = await GetCartAsync(userId, sessionId);
            return cart?.Items.Sum(ci => ci.Quantity) ?? 0;
        }

        public async Task<bool> IsProductInCartAsync(string userId, int productId, string? sessionId = null)
        {
            var cart = await GetCartAsync(userId, sessionId);
            return cart?.Items.Any(ci => ci.ProductId == productId) ?? false;
        }

        public async Task<(bool Success, string Message)> ApplyPromoCodeAsync(string userId, string promoCode, string? sessionId = null)
        {
            try
            {
                var cart = await GetCartAsync(userId, sessionId);
                if (cart == null)
                {
                    return (false, "Giỏ hàng không tồn tại.");
                }

                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Code == promoCode && p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

                if (promotion == null)
                {
                    return (false, "Mã khuyến mãi không hợp lệ hoặc đã hết hạn.");
                }

                // Check if promotion has been used by this user
                var usageCount = await _context.PromotionUsages
                    .CountAsync(pu => pu.PromotionId == promotion.Id && pu.UserId == userId);

                if (promotion.MaxUsagePerUser.HasValue && usageCount >= promotion.MaxUsagePerUser.Value)
                {
                    return (false, "Bạn đã sử dụng hết số lần được phép cho mã khuyến mãi này.");
                }

                // Check if promotion is first time only and user has used it before
                if (promotion.IsFirstTimeOnly && usageCount > 0)
                {
                    return (false, "Mã khuyến mãi này chỉ dành cho lần mua đầu tiên.");
                }

                // Apply promotion to cart
                cart.PromotionCode = promoCode;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, $"Mã khuyến mãi '{promoCode}' đã được áp dụng thành công!");
            }
            catch (Exception)
            {
                return (false, "Có lỗi xảy ra khi áp dụng mã khuyến mãi.");
            }
        }
    }
}
