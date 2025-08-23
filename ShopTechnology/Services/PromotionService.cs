using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class PromotionService : IPromotionService
{
    private readonly ShopTechnologyAccessoriesContext _context;

    public PromotionService(ShopTechnologyAccessoriesContext context)
    {
        _context = context;
    }

    public async Task<List<Promotion>> GetAllPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.IsActive && p.EndDate > DateTime.Now)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Promotion?> GetPromotionByIdAsync(int id)
    {
        return await _context.Promotions
            .Include(p => p.ProductPromotions)
            .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.PromotionId == id);
    }

    public async Task<Promotion?> GetPromotionByCodeAsync(string code)
    {
        return await _context.Promotions
            .Include(p => p.ProductPromotions)
            .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.Code == code && p.IsActive && p.EndDate > DateTime.Now);
    }

    public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
    {
        promotion.CreatedAt = DateTime.Now;
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task<bool> UpdatePromotionAsync(Promotion promotion)
    {
        var existingPromotion = await _context.Promotions.FindAsync(promotion.PromotionId);
        if (existingPromotion == null) return false;

        existingPromotion.Name = promotion.Name;
        existingPromotion.Description = promotion.Description;
        existingPromotion.Code = promotion.Code;
        existingPromotion.DiscountType = promotion.DiscountType;
        existingPromotion.DiscountValue = promotion.DiscountValue;
        existingPromotion.MinimumOrderAmount = promotion.MinimumOrderAmount;
        existingPromotion.MaximumDiscountAmount = promotion.MaximumDiscountAmount;
        existingPromotion.UsageLimit = promotion.UsageLimit;
        existingPromotion.StartDate = promotion.StartDate;
        existingPromotion.EndDate = promotion.EndDate;
        existingPromotion.IsActive = promotion.IsActive;
        existingPromotion.IsPublic = promotion.IsPublic;
        existingPromotion.ImageUrl = promotion.ImageUrl;
        existingPromotion.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null) return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount)
    {
        return await ValidatePromotionCodeAsync(code, orderAmount);
    }

    public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
    {
        var promotion = await GetPromotionByCodeAsync(code);
        if (promotion == null) return 0;

        if (!await ValidatePromotionCodeAsync(code, orderAmount))
            return 0;

        decimal discount = 0;

        if (promotion.DiscountType == "Percentage")
        {
            discount = orderAmount * (promotion.DiscountValue / 100);
        }
        else if (promotion.DiscountType == "FixedAmount")
        {
            discount = promotion.DiscountValue;
        }

        // Kiểm tra giới hạn tối đa
        if (promotion.MaximumDiscountAmount.HasValue && discount > promotion.MaximumDiscountAmount.Value)
        {
            discount = promotion.MaximumDiscountAmount.Value;
        }

        // Đảm bảo discount không vượt quá order amount
        if (discount > orderAmount)
        {
            discount = orderAmount;
        }

        return discount;
    }

    public async Task<bool> UsePromotionAsync(string code)
    {
        return await IncrementUsageCountAsync(code);
    }

    public async Task<List<Promotion>> GetActivePromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.IsActive && p.IsPublic && p.StartDate <= DateTime.Now && p.EndDate > DateTime.Now)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetExpiredPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.EndDate < DateTime.Now)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetUpcomingPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.IsActive && p.StartDate > DateTime.Now)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<bool> TogglePromotionStatusAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null) return false;

        promotion.IsActive = !promotion.IsActive;
        promotion.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalPromotionsCountAsync()
    {
        return await _context.Promotions.CountAsync();
    }

    public async Task<int> GetActivePromotionsCountAsync()
    {
        return await _context.Promotions
            .CountAsync(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate > DateTime.Now);
    }

    public async Task<decimal> GetTotalDiscountUsedAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        return await query.SumAsync(o => o.DiscountAmount);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var query = _context.Promotions.Where(p => p.Code == code);

        if (excludeId.HasValue)
            query = query.Where(p => p.PromotionId != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<List<Promotion>> GetPromotionsByProductIdAsync(int productId)
    {
        return await _context.Promotions
            .Include(p => p.ProductPromotions)
            .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate > DateTime.Now &&
                       p.ProductPromotions.Any(pp => pp.ProductId == productId))
            .ToListAsync();
    }

    public async Task<bool> ValidatePromotionCodeAsync(string code, decimal orderAmount)
    {
        var promotion = await GetPromotionByCodeAsync(code);
        if (promotion == null) return false;

        // Kiểm tra thời gian hiệu lực
        if (promotion.StartDate > DateTime.Now || promotion.EndDate < DateTime.Now)
            return false;

        // Kiểm tra số lần sử dụng
        if (promotion.UsageLimit.HasValue && promotion.UsedCount >= promotion.UsageLimit.Value)
            return false;

        // Kiểm tra giá trị đơn hàng tối thiểu
        if (promotion.MinimumOrderAmount.HasValue && orderAmount < promotion.MinimumOrderAmount.Value)
            return false;

        return true;
    }

    public async Task<bool> IncrementUsageCountAsync(string code)
    {
        var promotion = await GetPromotionByCodeAsync(code);
        if (promotion == null) return false;

        promotion.UsedCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateExpiredPromotionsAsync()
    {
        var expiredPromotions = await GetExpiredPromotionsAsync();
        foreach (var promotion in expiredPromotions)
        {
            promotion.IsActive = false;
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
