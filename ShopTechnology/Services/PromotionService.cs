using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            return await _context.Promotions
                .Where(p => p.IsActive && 
                           p.StartDate <= DateTime.UtcNow && 
                           p.EndDate >= DateTime.UtcNow)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            return await _context.Promotions
                .Include(p => p.ProductPromotions)
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _context.Promotions
                .Include(p => p.ProductPromotions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CreatePromotionAsync(Promotion promotion)
        {
            try
            {
                promotion.CreatedAt = DateTime.UtcNow;
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            try
            {
                promotion.UpdatedAt = DateTime.UtcNow;
                _context.Promotions.Update(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null) return false;

                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidatePromotionAsync(string code, string userId, decimal orderAmount)
        {
            var promotion = await GetPromotionByCodeAsync(code);
            if (promotion == null) return false;

            // Check if promotion is active and within date range
            if (!promotion.IsActive || 
                promotion.StartDate > DateTime.UtcNow || 
                promotion.EndDate < DateTime.UtcNow)
                return false;

            // Check minimum order amount
            if (promotion.MinimumOrderAmount.HasValue && 
                orderAmount < promotion.MinimumOrderAmount.Value)
                return false;

            // Check usage limit
            if (promotion.UsageLimit.HasValue && 
                promotion.UsedCount >= promotion.UsageLimit.Value)
                return false;

            // Check if user has already used this promotion (for first-time only)
            if (promotion.IsFirstTimeOnly)
            {
                var hasUsed = await _context.PromotionUsages
                    .AnyAsync(pu => pu.PromotionId == promotion.Id && pu.UserId == userId);
                if (hasUsed) return false;
            }

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
        {
            var promotion = await GetPromotionByCodeAsync(code);
            if (promotion == null) return 0;

            decimal discount = 0;

            switch (promotion.Type)
            {
                case PromotionType.Percentage:
                    discount = orderAmount * (promotion.Value / 100);
                    break;
                case PromotionType.FixedAmount:
                    discount = promotion.Value;
                    break;
                case PromotionType.FreeShipping:
                    discount = 10; // Assuming shipping cost is $10
                    break;
            }

            // Apply maximum discount limit
            if (promotion.MaximumDiscountAmount.HasValue && 
                discount > promotion.MaximumDiscountAmount.Value)
            {
                discount = promotion.MaximumDiscountAmount.Value;
            }

            return Math.Min(discount, orderAmount); // Don't discount more than order amount
        }

        public async Task<bool> ApplyPromotionToOrderAsync(int orderId, string code)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                var discount = await CalculateDiscountAsync(code, order.Subtotal);
                if (discount <= 0) return false;

                order.DiscountAmount = discount;
                order.TotalAmount = order.Subtotal + order.TaxAmount + order.ShippingAmount - discount;
                // order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IncrementUsageCountAsync(string code)
        {
            try
            {
                var promotion = await GetPromotionByCodeAsync(code);
                if (promotion == null) return false;

                promotion.UsedCount++;
                promotion.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByProductAsync(int productId)
        {
            return await _context.Promotions
                .Include(p => p.ProductPromotions)
                .Where(p => p.IsActive && 
                           p.StartDate <= DateTime.UtcNow && 
                           p.EndDate >= DateTime.UtcNow &&
                           p.ProductPromotions.Any(pp => pp.ProductId == productId))
                .ToListAsync();
        }

        public async Task<bool> AddProductToPromotionAsync(int promotionId, int productId)
        {
            try
            {
                var existing = await _context.ProductPromotions
                    .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && pp.ProductId == productId);
                
                if (existing != null) return true; // Already exists

                var productPromotion = new ProductPromotion
                {
                    PromotionId = promotionId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductPromotions.Add(productPromotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveProductFromPromotionAsync(int promotionId, int productId)
        {
            try
            {
                var productPromotion = await _context.ProductPromotions
                    .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && pp.ProductId == productId);
                
                if (productPromotion == null) return false;

                _context.ProductPromotions.Remove(productPromotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
